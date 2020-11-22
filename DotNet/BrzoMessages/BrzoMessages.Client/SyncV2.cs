using BrzoMessages.Client.dto;
using BrzoMessages.Client.NatsConn;
using Newtonsoft.Json;
using System;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Websocket.Client;

namespace BrzoMessages.Client
{
    internal class SyncV2 : ConnectionNats
    {
        public event DelegateHandlerMessages HandlerMessages;
        public event DelegateHandlerAck HandlerAck;
        public event DelegateHandlerLogs HandlerLogs;

        private string privateKey;
        private string keyAccess;

        private SyncV2(string keyAccess, string privateKey, bool synchronous = true) : base(keyAccess, privateKey, synchronous)
        {
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
        }

        public void Connect()
        {
            if (HandlerMessages == null)
                throw new Exception("HandlerMessages não registrado");
            connect();
        }

        public bool IsConnected
        {
            get
            {
                return connected;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return false;
            }
        }

        protected override void MessageReceived(MessageReceived message, IWebsocketClient client)
        {
            try
            {
                if (message != null)
                {
                    var result = HandlerMessages?.Invoke(message);
                    if (result.HasValue && result.Value)
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                using (var c = new ConfirmMessage(this.keyAccess, this.privateKey))
                                {
                                    c.Ok(message.data.Info.Id, message.data.Info.RemoteJid);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        });
                        Task.Run(() =>
                        {
                            try
                            {
                                client.Send(JsonConvert.SerializeObject(new
                                {
                                    Token = keyAccess,
                                    message.data.Info.Id,
                                    message.data.Info.RemoteJid
                                }));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        protected override void MessageAck(MessageAck message)
        {
            try
            {
                if (message != null)
                {
                    HandlerAck?.Invoke(message);
                    //if (result.HasValue && result.Value)
                    //{
                    //    using (var c = new ConfirmMessage(this.keyAccess, this.privateKey))
                    //    {
                    //        c.Ok(message.ID, message.To);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Dispose();
        }
        private void DefaultOnUnloading(AssemblyLoadContext obj)
        {
            Dispose();
        }
        private void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }
        protected override void DisconnectionHappened(Exception exception)
        {
            Console.WriteLine(exception?.Message);
        }

        protected override void Logs(string log)
        {
            HandlerLogs?.Invoke(log);
        }

    }
}
