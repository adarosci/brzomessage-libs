using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace BrzoMessages.Client
{
    public abstract class ConnectionSync : IDisposable
    {
        private readonly string keyAccess;
        private readonly string privateKey;
        private readonly bool asynchronous;
        private readonly ManualResetEvent ExitEvent;
        private readonly Uri url;

        public ConnectionSync(string keyAccess, string privateKey, bool asynchronous)
        {
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;
            this.asynchronous = asynchronous;
            ExitEvent = new ManualResetEvent(false);
            url = new Uri($"{Config.SOCKET_URL}/{keyAccess}?p={privateKey}");
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
                    }
                };
                return client;
            });

            Task.Run(() =>
            {
                using (IWebsocketClient client = new WebsocketClient(url, factory))
                {
                    client.Name = "Bitmex";
                    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ReconnectionHappened.Subscribe(type =>
                    {
                        Console.WriteLine($"Reconnection happened, type: {type}, url: {client.Url}");
                    });
                    client.DisconnectionHappened.Subscribe(info =>
                    {
                        Console.WriteLine($"Disconnection happened, type: {info}");
                       // DisconnectionHappened(info.Exception);
                    });
                    client.MessageReceived.Subscribe(msg =>
                    {
                        try
                        {
                            Console.WriteLine($"Message received: {msg}");

                            var obj = JsonConvert.DeserializeObject<dto.MessageReceivedSocket>(msg.Text).body.Data;
                            var data = JsonConvert.DeserializeObject<dto.MessageReceived>(obj);

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
                        catch (Exception)
                        {

                        }
                    });

                    client.Start().Wait();

                    Task.Run(() => StartSendingPing(client));
                    Task.Run(() => SwitchUrl(client));

                    using (var c = new ConnectionStart(keyAccess, privateKey))
                    {
                        c.Connect(keyAccess);
                    }

                    ExitEvent.WaitOne();
                }
            });
        }

        protected abstract void DisconnectionHappened(Exception exception);
        protected abstract void MessageReceived(dto.MessageReceived message);

        private async Task StartSendingPing(IWebsocketClient client)
        {
            while (true)
            {
                if (client == null)
                    break;

                await Task.Delay(1000);

                if (!client.IsRunning)
                    continue;

                client.Send("ping");
            }
        }

        private async Task SwitchUrl(IWebsocketClient client)
        {
            while (true)
            {
                if (client == null)
                    break;

                await Task.Delay(20000);
                client.Url = url;
                await client.Reconnect();
            }
        }

        public void Dispose()
        {
            ExitEvent.Set();
            //using (var c = new ConnectionStop(keyAccess, privateKey))
            //{
            //    c.Disconnect(keyAccess);
            //}
        }
    }
}
