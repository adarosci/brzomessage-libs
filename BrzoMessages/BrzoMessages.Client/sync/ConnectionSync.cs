﻿using BrzoMessages.Client.Exceptions;
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
        private string lastAck;
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

            try
            {
                var auth = new ConnectionAuth(keyAccess, privateKey).Authenticate();
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
                                Logs($"Reconnection happened url: {client.Url} {DateTime.Now}");
                            });
                            client.DisconnectionHappened.Subscribe(info =>
                            {
                                if (info.Exception != null)
                                {
                                    Logs($"Disconnection happened, type: {info.Exception?.Message}");
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
                                        if (data != null && data.data != null && data.data.Info.Id != lastMessageId)
                                        {
                                            lastVersion = obj.version;
                                            lastMessageId = data.data.Info.Id;

                                            if (asynchronous)
                                                MessageReceived(data);
                                            else
                                                Task.Run(() => MessageReceived(data));
                                        }
                                        else
                                        {
                                            var ack = JsonConvert.DeserializeObject<dto.MessageAck>(obj.body.Data);
                                            if (ack != null && lastAck != (ack.ID + ack.Ack))
                                            {
                                                lastAck = ack.ID + ack.Ack;
                                                if (asynchronous)
                                                    MessageAck(ack);
                                                else
                                                    Task.Run(() => MessageAck(ack));
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
                    }
                    if (!dispose)
                        connect();
                });
            }
            catch (TimeoutException ex)
            {
                Logs(ex.Message);
            }
            catch (AuthException ex)
            {
                Logs(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Logs("Error auth retry in 5 secounds");
                Task.Delay(5000).Wait();

                connect();
            }
        }

        protected abstract void DisconnectionHappened(Exception exception);
        protected abstract void Logs(string log);
        protected abstract void MessageReceived(dto.MessageReceived message);
        protected abstract void MessageAck(dto.MessageAck message);

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