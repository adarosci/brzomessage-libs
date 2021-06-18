using BrzoMessages.Client.dto;
using BrzoMessages.Client.NatsConn;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;

namespace BrzoMessages.Client
{
    public class Sync
    {
        public event DelegateHandlerMessages HandlerMessages;
        public event DelegateHandlerAck HandlerAck;
        public event DelegateHandlerJSON HandlerJSON;

        private SyncV2 v2;
        private SyncOld v1;

        public Sync(string keyAccess, string privateKey, bool synchronous = true, string baseURL = null)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            var config = Config.NewConfig(baseURL);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {privateKey}");

            var result = client.PostAsync($"{config.MAGANER_URL}/api/messages/connect?token={keyAccess}", new StringContent("", Encoding.UTF8, "application/json")).Result;
            
            if (result.StatusCode == System.Net.HttpStatusCode.HttpVersionNotSupported)
            {
                v1 = new SyncOld(keyAccess, privateKey, synchronous);
                v1.HandlerAck += V2_HandlerAck;
                v1.HandlerJSON += V2_HandlerJSON;
                v1.HandlerMessages += V2_HandlerMessages;
                //v1.Connect();
                return;
            }

            v2 = new SyncV2(keyAccess, privateKey, baseURL);
            v2.HandlerAck += V2_HandlerAck;
            v2.HandlerJSON += V2_HandlerJSON;
            v2.HandlerMessages += V2_HandlerMessages;
            //v2.Connect();
        }

        public void Connect()
        {
            if (v1 != null) v1.Connect();
            if (v2 != null) v2.Connect();
        }

        private bool V2_HandlerMessages(MessageReceived message)
        {
            var vl = HandlerMessages?.Invoke(message);
            if (vl.HasValue)
            {
                return vl.Value;
            }
            return false;
        }

        private bool V2_HandlerJSON(string message)
        {
            var vl = HandlerJSON?.Invoke(message);
            if (vl.HasValue)
            {
                return vl.Value;
            }
            return false;
        }

        private bool V2_HandlerAck(MessageAck message)
        {
            var vl = HandlerAck?.Invoke(message);
            if (vl.HasValue)
            {
                return vl.Value;
            }
            return false;
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            //Dispose();
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            //Dispose();
        }
    }

    internal class SyncV2 : ConnectionNats
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
