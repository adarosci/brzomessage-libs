using BrzoMessages.Client.dto;
using BrzoMessages.Client.NatsConn;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace BrzoMessages.Client
{
    public class SyncV2 : ConnectionNats
    {
        public event DelegateHandlerMessages HandlerMessages;
        public event DelegateHandlerAck HandlerAck;
        public event DelegateHandlerJSON HandlerJSON;

        private string privateKey;
        private string keyAccess;

        public SyncV2(string keyAccess, string privateKey, string baseURL = null) : base(keyAccess, privateKey, baseURL)
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
                return false;
            }
        }

        protected override bool MessageReceived(MessageReceived message)
        {
            try
            {
                if (message != null)
                {
                    var result = HandlerMessages?.Invoke(message);
                    if (result == null)
                    {
                        return false;
                    }
                    return result.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return false;
        }


        protected override void MessageAck(MessageAck message)
        {
            try
            {
                if (message != null)
                {
                    HandlerAck?.Invoke(message);
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

    }
}
