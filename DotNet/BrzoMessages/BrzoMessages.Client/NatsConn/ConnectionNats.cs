using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrzoMessages.Client.Exceptions;
using NATS.Client;
using Websocket.Client;

namespace BrzoMessages.Client.NatsConn
{
    public abstract class ConnectionNats : IDisposable
    {
        private bool dispose;
        private string keyAccess;
        private string privateKey;
        private bool asynchronous;
        private IConnection c;
        private LoginToken client;
        private IAsyncSubscription messageReceived;
        private IAsyncSubscription messageJson;
        private IAsyncSubscription ping;
        protected bool connected;
        private object _lock = new object();

        class LoginToken
        {
            public Guid Token { get; set; }
            public string PrivateKey { get; set; }
            public Guid ClientID { get; set; }
            public int PersonSessionWhatsappID { get; set; }
        }
        class ErrorRequest
        {
            public string Error { get; set; }
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

        public ConnectionNats(string keyAccess, string privateKey, bool asynchronous)
        {
            this.dispose = false;
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;
            this.asynchronous = asynchronous;
        }

        protected abstract void DisconnectionHappened(Exception exception);
        protected abstract void Logs(string log);
        protected abstract void MessageReceived(dto.MessageReceived message, IWebsocketClient client);
        protected abstract void MessageAck(dto.MessageAck message);

        protected void connect()
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    Options opts = ConnectionFactory.GetDefaultOptions();
                    opts.Url = Config.NATS_URL;

                    opts.ServerDiscoveredEventHandler += (sender, args) =>
                    {
                        Console.WriteLine("A new server has joined the cluster:");
                        Console.WriteLine("    " + String.Join(", ", args.Conn.DiscoveredServers));
                    };

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

                    var token = new LoginToken
                    {
                        PrivateKey = privateKey,
                        Token = Guid.Parse(keyAccess)
                    };

                    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new CallRequest
                    {
                        Data = Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(token))
                    });

                    var msg = c.Request("client.connect.INTERNAL", Encoding.ASCII.GetBytes(payload), 30000);

                    string someString = Encoding.ASCII.GetString(msg.Data);

                    var error = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorRequest>(someString);
                    if (error != null && error.Code > 0)
                    {
                        throw new AuthException(error.Error);
                    }

                    client = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginToken>(someString);

                    Console.WriteLine("client." + client.ClientID.ToString() + ".message.received.INTERNAL");
                    messageReceived = c.SubscribeAsync("client." + client.ClientID.ToString() + ".message.received.INTERNAL", (sender, args) =>
                    {
                        var data = Encoding.ASCII.GetString(args.Message.Data);

                        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<CallRequest>(data);

                        var mensagem = Encoding.ASCII.GetString(obj.Data);

                        Console.WriteLine(mensagem);

                        var msgData = Newtonsoft.Json.JsonConvert.DeserializeObject<dto.MessageReceived>(mensagem);

                        c.Publish(args.Message.Reply, Encoding.ASCII.GetBytes(""));
                    });

                    Console.WriteLine("client." + client.ClientID.ToString() + ".message.json.INTERNAL");
                    messageJson = c.SubscribeAsync("client." + client.ClientID.ToString() + ".message.json.INTERNAL", (sender, args) =>
                    {
                        var data = Encoding.ASCII.GetString(args.Message.Data);

                        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<CallRequest>(data);

                        var mensagem = Encoding.ASCII.GetString(obj.Data);

                        var msgData = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageJson>(mensagem);

                        Console.WriteLine(msgData.Message);

                        c.Publish(args.Message.Reply, Encoding.ASCII.GetBytes(""));
                    });

                    Console.WriteLine("client." + client.ClientID.ToString() + ".ping.INTERNAL");
                    ping = c.SubscribeAsync("client." + client.ClientID.ToString() + ".ping.INTERNAL", (sender, args) =>
                    {
                        c.Publish(args.Message.Reply, Encoding.ASCII.GetBytes(""));
                    });

                    var tokenSource1 = new CancellationTokenSource();

                    Task.Run(() => StartSendingPing(), tokenSource1.Token);

                    connected = true;
                }
                catch (TimeoutException ex)
                {
                    Logs(ex.Message);
                    Logs("Error auth retry in 5 secounds");
                    Task.Delay(5000).Wait();

                    if (!dispose)
                        Task.Run(() => disposeReconnect(ex.Message));
                }
                catch (AuthException ex)
                {
                    Logs(ex.Message);
                    Logs("Error auth retry in 5 secounds");
                    Task.Delay(5000).Wait();

                    if (!dispose)
                        Task.Run(() => disposeReconnect(ex.Message));
                }
                catch (Exception ex)
                {
                    Logs("Error auth retry in 5 secounds");
                    Task.Delay(5000).Wait();
                    if (!dispose)
                        Task.Run(() => disposeReconnect(ex.Message));
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
                    if (c == null || c.IsClosed())
                    {
                        break;
                    }

                    var msg = c.Request("client.alive", Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(client)));
                    var data = Encoding.ASCII.GetString(msg.Data);

                    var err = Newtonsoft.Json.JsonConvert.DeserializeObject<ErrorRequest>(data);
                    if (err != null && err.Code > 0)
                    {
                        Logs("Error ping in 5 secounds");
                        Task.Delay(5000).Wait();

                        if (!dispose)
                            Task.Run(() => disposeReconnect(err.Error));

                        break;
                    }

                    Logs(data);
                }
                catch (TimeoutException ex)
                {
                    break;
                }
                catch (Exception)
                {
                    break;
                }

                await Task.Delay(30000);
            }
        }


        private void disposeReconnect(string error)
        {
            connected = false;
            Dispose();
            Logs(error);
            if (!dispose)
                Task.Run(() => connect());
        }

        public void Dispose()
        {
            try
            {
                messageJson?.Unsubscribe();
                messageReceived?.Unsubscribe();
                ping?.Unsubscribe();

                if (!c.IsClosed())
                {
                    // Draining and closing a connection
                    c?.Drain();
                    // Closing a connection
                    c?.Close();
                }
            }
            catch
            {

            }
        }
    }
}
