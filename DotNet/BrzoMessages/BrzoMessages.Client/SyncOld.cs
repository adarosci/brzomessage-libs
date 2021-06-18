using BrzoMessages.Client.dto;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace BrzoMessages.Client
{
    

    public delegate bool DelegateHandlerMessages(MessageReceived message);
    public delegate bool DelegateHandlerAck(MessageAck message);
    public delegate bool DelegateHandlerJSON(string message);
    public delegate void DelegateHandlerLogs(string message);

    internal class SyncOld : ConnectionSync
    {
        public event DelegateHandlerMessages HandlerMessages;
        public event DelegateHandlerAck HandlerAck;
        public event DelegateHandlerJSON HandlerJSON;
        public event DelegateHandlerLogs HandlerLogs;

        private string privateKey;
        private string keyAccess;

        public SyncOld(string keyAccess, string privateKey, bool synchronous = true) : base(keyAccess, privateKey, synchronous)
        {
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
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
                return dispose;
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

        protected override void MessageJSON(string message)
        {
            try
            {
                if (message != null)
                {
                    HandlerJSON?.Invoke(message);
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
