using BrzoMessages.Client;
using BrzoMessages.Client.dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BrzoMessages.Teste
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //var s = new Sync("cad587f6-4f06-4c9f-9575-ae500b5f161c", "DOvJHQ-CSB-tBs-u2HhE6RhwT2t6nZZ7");
            //s.HandlerMessages += S_HandlerMessages;
            //s.HandlerLogs += S_HandlerLogs;
            //s.Connect();

            //Thread.Sleep(Timeout.Infinite);
        }

        private void S_HandlerLogs(string message)
        {
            Console.WriteLine(message);
        }

        private bool S_HandlerMessages(MessageReceived message)
        {
            Console.WriteLine(message);
            return true;
        }

        [TestMethod]
        public void TestNewMessage()
        {
            var send = new SendMessage("cad587f6-4f06-4c9f-9575-ae500b5f161c", "DOvJHQ-CSB-tBs-u2HhE6RhwT2t6nZZ7");
            var (ok, code, error) = send.Text(new MessageText
            {
                Id = Guid.NewGuid(),
                Destiny = new List<string>() { "554188304865" },
                Text = "Oi teste C#"
            });
            Console.WriteLine("", ok, code, error);
            Assert.IsTrue(ok);
        }
    }
}
