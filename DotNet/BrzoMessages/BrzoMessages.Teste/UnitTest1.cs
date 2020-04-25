using BrzoMessages.Client;
using BrzoMessages.Client.dto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            var list = new List<int>() { 1, 2, 3, 4, 5 };
            list.AsParallel().ForAll(x =>
            {
                var send = new SendMessage("526f72cc-74e7-4b25-9cbf-7a6755fc3c76", "yfPSI4ur6WyrYtdUBewtX5qOX14_9TbC");
                var (ok, code, error) = send.Text(new MessageText
                {
                    Id = Guid.NewGuid(),
                    Destiny = new List<string>() { "554196584089" },
                    Text = "Teste CT Async " + x
                });
                Console.WriteLine("", ok, code, error);

            });
            Assert.IsTrue(true);
        }
    }
}
