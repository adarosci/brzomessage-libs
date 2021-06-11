using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrzoMessages.Client.Exceptions;
using NATS.Client;
using Newtonsoft.Json;
using Websocket.Client;

namespace BrzoMessages.Client.NatsConn
{
    public abstract class ConnectionNats : IDisposable
    {
        private string containerID;
        private string baseURL;
        private bool dispose;
        private string keyAccess;
        private string privateKey;
        private IConnection c;
        private IAsyncSubscription messageReceived;
        private IAsyncSubscription messageDisconnect;
        protected bool connected;
        private object _lock = new object();

        class ErrorRequest
        {
            public string Message { get; set; }
            public int Code { get; set; }
        }
        class CallRequest
        {
            public Dictionary<string, string> Params { get; set; }
            public byte[] Data { get; set; }
        }
        class MessageJson
        {
            public string Message { get; set; }
        }

        public ConnectionNats(string keyAccess, string privateKey, string baseURL)
        {
            this.baseURL = baseURL;
            this.dispose = false;
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;

            Task.Run(() => StartSendingPing());
        }

        protected abstract bool MessageReceived(dto.MessageReceived message);
        protected abstract void MessageAck(dto.MessageAck message);
        protected abstract void MessageJSON(string message);

        protected void connect()
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    Console.WriteLine("Conectando...");

                    var config = Config.NewConfig(this.baseURL);

                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {privateKey}");

                    var result = client.PostAsync($"{config.MAGANER_URL}/api/messages/connect?token={keyAccess}", new StringContent("", Encoding.UTF8, "application/json")).Result;
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        throw new Exception("Não foi possivel conectar", new Exception(result.StatusCode.ToString()));
                    }

                    Options opts = ConnectionFactory.GetDefaultOptions();

                    opts.Url = config.NATS_URL;

                    opts.ClosedEventHandler += (sender, args) =>
                    {
                        Console.WriteLine("Connection Closed: ");
                        Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);

                        disposeReconnect("connection closed");
                    };

                    opts.DisconnectedEventHandler += (sender, args) =>
                    {
                        Console.WriteLine("Connection Disconnected: ");
                        Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);

                        disposeReconnect("Connection Disconnected");
                    };

                    c = new ConnectionFactory().CreateConnection(opts);

                    containerID = result.Content.ReadAsStringAsync().Result;

                    Console.WriteLine($"Connected to containerID: {containerID}");

                    var messagesChannel = $"whatsapp.{containerID}.message.INTERNAL";
                    var messagesDisconnect = $"whatsapp.{containerID}.disconnect.INTERNAL";

                    messageDisconnect = c.SubscribeAsync(messagesDisconnect, (sender, args) =>
                    {
                        disposeReconnect("Desconectado pelo servidor");
                    });

                    messageReceived = c.SubscribeAsync(messagesChannel, (sender, args) =>
                    {
                        try
                        {
                            var data = Encoding.ASCII.GetString(args.Message.Data);

                            var obj = JsonConvert.DeserializeObject<CallRequest>(data);

                            var mensagem = Encoding.ASCII.GetString(obj.Data);

                            var msgData = JsonConvert.DeserializeObject<dto.Message>(mensagem);

                            if (msgData.type == "MESSAGE.RECEIVED")
                            {
                                var message = JsonConvert.DeserializeObject<dto.MessageReceived>(JsonConvert.SerializeObject(msgData));
                                var ok = MessageReceived(message);
                                if (!ok) return;
                            }
                            else if (msgData.type == "MESSAGE.ACK")
                            {
                                var message = JsonConvert.DeserializeObject<dto.MessageAck>(JsonConvert.SerializeObject(msgData.data));
                                MessageAck(message);
                            }
                            else
                            {
                                MessageJSON(JsonConvert.SerializeObject(msgData.data));
                            }

                            c.Publish(args.Message.Reply, Encoding.ASCII.GetBytes(""));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    });

                    var messagesConnect = $"whatsapp.{containerID}.connect.INTERNAL";
                    c.Publish(messagesConnect, null);

                    connected = true;
                }
                catch (TimeoutException ex)
                {
                    disposeReconnect(ex.Message);
                }
                catch (AuthException ex)
                {
                    disposeReconnect(ex.Message);
                }
                catch (Exception ex)
                {
                    disposeReconnect(ex.Message);
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        private async Task StartSendingPing()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Ping");
                    if (c == null)
                    {
                        continue;
                    }

                    if (c.IsClosed())
                    {
                        disposeReconnect("container invalido");
                        continue;
                    }

                    var channelAlive = $"whatsapp.{containerID}.alive.GET";

                    var msg = c.Request(channelAlive, null);
                    var data = Encoding.ASCII.GetString(msg.Data);

                    if (containerID.Contains(data))
                    {
                        disposeReconnect("container invalido");
                        continue;
                    }

                }
                catch (Exception ex)
                {
                    disposeReconnect("container invalido");
                    continue;
                }
                finally
                {
                    await Task.Delay(10000);
                }
            }
        }


        private void disposeReconnect(string error)
        {
            Console.WriteLine($"Desconectado: ${error}");
            Dispose();
            Console.WriteLine($"Reconectando em 5 segundos...");
            Task.Delay(5000).Wait();
            Task.Run(() => connect());
        }

        public void Dispose()
        {
            try
            {
                messageReceived?.Unsubscribe();
                messageDisconnect?.Unsubscribe();

                // Draining and closing a connection
                c?.Drain();
                // Closing a connection
                c?.Close();

                c = null;
            }
            catch
            {

            }
        }
    }
}
