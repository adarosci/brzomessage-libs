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
            var send = new SendMessage("token", "privateKey");
            var (ok, code, error) = send.Text(new MessageText
            {
                Id = Guid.NewGuid(),
                Destiny = new List<string>() { "5541...." },
                Text = "Texto"
            });
            Console.WriteLine("", ok, code, error);
            Assert.IsTrue(ok);
        }
    }
}
