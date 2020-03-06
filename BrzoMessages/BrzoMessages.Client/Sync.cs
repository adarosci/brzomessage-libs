using BrzoMessages.Client.dto;
using System;
using System.Runtime.Loader;

namespace BrzoMessages.Client
{
    public delegate void DelegateConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e);
    public delegate void DelegateDefaultOnUnloading(AssemblyLoadContext obj);
    public delegate void DelegateCurrentDomainOnProcessExit(object sender, EventArgs e);
    public delegate void DelegateDisconnectionHappened(Exception exception);

    public class Sync : ConnectionSync
    {
        public event DelegateConsoleOnCancelKeyPress EventConsoleOnCancelKeyPress;
        public event DelegateDefaultOnUnloading EventDefaultOnUnloading;
        public event DelegateCurrentDomainOnProcessExit EventCurrentDomainOnProcessExit;
        public event DelegateDisconnectionHappened EventErrorConnectionOrMessageProcessed;

        private Func<MessageReceived, bool> handler;

        private string privateKey;
        private string keyAccess;

        public Sync(string keyAccess, string privateKey, bool synchronous = true) : base(keyAccess, privateKey, synchronous)
        {
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomainOnProcessExit;
            AssemblyLoadContext.Default.Unloading += DefaultOnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
        }

        public void Connect(Func<MessageReceived, bool> handler)
        {
            this.handler = handler;
            connect();
        }

        protected override void MessageReceived(MessageReceived message)
        {
            try
            {
                if (handler(message))
                {
                    //using (var c = new ConfirmMessage(this.keyAccess, this.privateKey))
                    //{
                    //    c.Ok(message);
                    //}
                }
            }
            catch (Exception ex)
            {
                EventErrorConnectionOrMessageProcessed?.Invoke(ex);
                Console.WriteLine(ex);
            }
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            EventConsoleOnCancelKeyPress?.Invoke(sender, e);
            Dispose();
        }
        private void DefaultOnUnloading(AssemblyLoadContext obj)
        {
            EventDefaultOnUnloading?.Invoke(obj);
            Dispose();
        }
        private void CurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            EventCurrentDomainOnProcessExit?.Invoke(sender, e);
            Dispose();
        }

        protected override void DisconnectionHappened(Exception exception) => EventErrorConnectionOrMessageProcessed?.Invoke(exception);
    }
}
