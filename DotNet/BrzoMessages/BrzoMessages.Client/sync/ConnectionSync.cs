using BrzoMessages.Client.Exceptions;
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
        protected bool dispose;
        private readonly string keyAccess;
        private readonly string privateKey;
        private readonly bool asynchronous;
        private ManualResetEvent ExitEvent;
        private readonly Uri url;
        private string auth;
        private Uri urlAuth;
        protected bool connected;
        private string lastMessageId;

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
            this.connected = false;
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

            try
            {
                auth = new ConnectionAuth(keyAccess, privateKey).Authenticate();
                urlAuth = new Uri(url + $"?token={keyAccess}&p={auth}");

                Task.Run(() =>
                {
                    ExitEvent = new ManualResetEvent(false);

                    if (auth == "")
                    {
                        Logs("Error auth retry in 5 secounds");
                        Task.Delay(5000).Wait();
                    }
                    else
                    {
                        using (IWebsocketClient client = new WebsocketClient(urlAuth, factory))
                        {
                            client.Name = "Bitmex";
                            client.ReconnectTimeout = TimeSpan.FromMinutes(5);
                            client.ErrorReconnectTimeout = TimeSpan.FromMinutes(5);
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
                                Logs($"Reconnection happened url: {client.Url} {DateTime.Now}");
                            });
                            client.DisconnectionHappened.Subscribe(info =>
                            {
                                if (info.Exception != null)
                                {
                                    Logs($"Disconnection happened, type: {info.Exception?.Message}");
                                    using (var c = new ConnectionStop(keyAccess, privateKey))
                                    {
                                        c.Disconnect(keyAccess, auth);
                                    }
                                    this.connected = false;
                                    ExitEvent.Set();
                                }
                            });
                            client.MessageReceived.Subscribe(msg =>
                            {
                                try
                                {
                                    if (!msg.Text.Equals("ping"))
                                    {
                                        var obj = JsonConvert.DeserializeObject<dto.MessageReceivedSocket>(msg.Text);

                                        var data = JsonConvert.DeserializeObject<dto.MessageReceived>(obj.body.Data);
                                        if (data != null && data.data != null)
                                        {
                                            lastMessageId = data.data.Info.Id;

                                            if (asynchronous)
                                                MessageReceived(data, client);
                                            else
                                                Task.Run(() => MessageReceived(data, client));
                                        }
                                        else
                                        {
                                            var ack = JsonConvert.DeserializeObject<dto.MessageAck>(obj.body.Data);
                                            if (ack != null)
                                            {
                                                if (ack.Ack == -1)
                                                {
                                                    MessageJSON(ack.To);
                                                }
                                                else if (asynchronous)
                                                    MessageAck(ack);
                                                else
                                                    Task.Run(() => MessageAck(ack));

                                                Task.Run(() =>
                                                {
                                                    client.Send(JsonConvert.SerializeObject(new
                                                    {
                                                        Token = keyAccess,
                                                        Id = ack.ID,
                                                        RemoteJid = ack.To
                                                    }));
                                                });
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
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
                    }
                    if (!dispose)
                        Task.Run(() => connect());
                });
            }
            catch (TimeoutException ex)
            {
                Logs(ex.Message);

                Logs("Error auth retry in 5 secounds");
                Task.Delay(5000).Wait();

                if (!dispose)
                    Task.Run(() => connect());
            }
            catch (AuthException ex)
            {
                Logs(ex.Message);

                Logs("Error auth retry in 5 secounds");
                Task.Delay(5000).Wait();

                if (!dispose)
                    Task.Run(() => connect());
            }
            catch (Exception ex)
            {
                Logs("Error auth retry in 5 secounds");
                Task.Delay(5000).Wait();

                if (!dispose)
                    Task.Run(() => connect());
            }
        }

        protected abstract void DisconnectionHappened(Exception exception);
        protected abstract void Logs(string log);
        protected abstract void MessageReceived(dto.MessageReceived message, IWebsocketClient client);
        protected abstract void MessageAck(dto.MessageAck message);
        protected abstract void MessageJSON(string message);

        private async Task StartSendingPing(IWebsocketClient client, CancellationToken cancellation)
        {
            while (true)
            {
                await Task.Delay(10000);
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
            dispose = true;
            using (var c = new ConnectionStop(keyAccess, privateKey))
            {
                c.Disconnect(keyAccess, auth);
            }
            ExitEvent.Set();
        }
    }
}
