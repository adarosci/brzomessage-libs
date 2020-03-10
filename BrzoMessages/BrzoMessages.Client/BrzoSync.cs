using BrzoMessages.Client.dto;
using System;
using System.Runtime.Loader;

namespace BrzoMessages.Client
{
    public delegate bool DelegateHandlerMessages(MessageReceived message);
    public delegate void DelegateHandlerLogs(string message);

    public class BrzoSync : ConnectionSync
    {
        public event DelegateHandlerMessages HandlerMessages;
        public event DelegateHandlerLogs HandlerLogs;

        private string privateKey;
        private string keyAccess;

        public BrzoSync(string keyAccess, string privateKey, bool synchronous = true) : base(keyAccess, privateKey, synchronous)
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

        protected override void MessageReceived(MessageReceived message)
        {
            try
            {
                if (message != null)
                {
                    var result = HandlerMessages?.Invoke(message);
                    if (result.HasValue && result.Value)
                    {
                        using (var c = new ConfirmMessage(this.keyAccess, this.privateKey))
                        {
                            c.Ok(message);
                        }
                    }
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
