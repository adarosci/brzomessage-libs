using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace BrzoMessages.Client
{
    public abstract class ConnectionSync : IDisposable
    {
        private bool dispose;
        private readonly string keyAccess;
        private readonly string privateKey;
        private readonly bool asynchronous;
        private ManualResetEvent ExitEvent;
        private readonly Uri url;
        private Uri urlAuth;
        private bool connected;
        private string lastMessageId;
        private int lastVersion;

        public ConnectionSync(string keyAccess, string privateKey, bool asynchronous)
        {
            this.dispose = false;
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;
            this.asynchronous = asynchronous;
            url = new Uri($"{Config.SOCKET_URL}/{keyAccess}");
        }

        protected void connect()
        {
            var factory = new Func<ClientWebSocket>(() =>
            {
                var client = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(5),
                        Credentials = new NetworkCredential(keyAccess, privateKey)
                    },
                };
                return client;
            });

            Task.Run(() =>
            {
                ExitEvent = new ManualResetEvent(false);

                var auth = new ConnectionAuth(keyAccess, privateKey).Authenticate();
                urlAuth = new Uri(url + $"?token={keyAccess}&p={auth}");


                using (IWebsocketClient client = new WebsocketClient(urlAuth, factory))
                {
                    client.Name = "Bitmex";
                    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ReconnectionHappened.Subscribe(type =>
                    {
                        if (!connected)
                        {
                            using (var c = new ConnectionStart(keyAccess, privateKey))
                            {
                                this.connected = true;
                                lastMessageId = c.Connect(keyAccess);
                            }
                        }
                        Console.WriteLine($"Reconnection happened url: {client.Url} {DateTime.Now}");
                    });
                    client.DisconnectionHappened.Subscribe(info =>
                    {
                        Console.WriteLine($"Disconnection happened, type: {info.Exception?.Message}");
                        if (info.Exception != null)
                        {
                            using (var c = new ConnectionStop(keyAccess, privateKey))
                            {
                                c.Disconnect(keyAccess);
                            }
                            this.connected = false;
                            ExitEvent.Set();
                        }
                    });
                    client.MessageReceived.Subscribe(msg =>
                    {
                        try
                        {
                            if (!msg.Text.Contains("ping"))
                            {
                                var obj = JsonConvert.DeserializeObject<dto.MessageReceivedSocket>(msg.Text);
                                var data = JsonConvert.DeserializeObject<dto.MessageReceived>(obj.body.Data);
                                
                                Console.WriteLine($"Message received: {DateTime.Now} - {data?.data?.Text}");

                                if (data != null && obj.version > lastVersion && data.data.Info.Id != lastMessageId)
                                {
                                    lastVersion = obj.version;
                                    lastMessageId = data.data.Info.Id;
                                    if (asynchronous)
                                    {
                                        MessageReceived(data);
                                    }
                                    else
                                    {
                                        Task.Run(() =>
                                        {
                                            MessageReceived(data);
                                        });
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    });

                    client.Start().Wait();

                    var tokenSource1 = new CancellationTokenSource();
                    var tokenSource2 = new CancellationTokenSource();

                    Task.Run(() => StartSendingPing(client, tokenSource1.Token), tokenSource1.Token);
                    Task.Run(() => SwitchUrl(client, tokenSource2.Token), tokenSource2.Token);

                    ExitEvent.WaitOne();

                    tokenSource1.Cancel();
                    tokenSource2.Cancel();
                }
                if (!dispose)
                    connect();
            });
        }

        protected abstract void DisconnectionHappened(Exception exception);
        protected abstract void MessageReceived(dto.MessageReceived message);

        private async Task StartSendingPing(IWebsocketClient client, CancellationToken cancellation)
        {
            while (true)
            {
                await Task.Delay(1000);
                if (cancellation.IsCancellationRequested)
                {
                    return;
                }

                if (!client.IsRunning)
                    continue;

                client.Send("ping");
            }
        }

        private async Task SwitchUrl(IWebsocketClient client, CancellationToken cancellation)
        {
            while (true)
            {
                await Task.Delay(200000);
                if (cancellation.IsCancellationRequested)
                {
                    return;
                }

                client.Url = urlAuth;
                await client.Reconnect();
            }
        }

        public void Dispose()
        {
            using (var c = new ConnectionStop(keyAccess, privateKey))
            {
                c.Disconnect(keyAccess);
            }
            ExitEvent.Set();
        }
    }
}
