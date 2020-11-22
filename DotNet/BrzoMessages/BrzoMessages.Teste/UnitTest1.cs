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
        [TestMethod]
        public void TestNewMessage()
        {
            var send = new SendMessage("token", "privateKey");
            var (ok, code, error) = send.Text(new MessageText
            {
                Id = Guid.NewGuid(),
                Destiny = new List<string>() { "5511..." },
                Text = "text",
                Context = new MessageContext
                {
                    MessageId = ""
                }
            });
            Console.WriteLine("", ok, code, error);
            Assert.IsTrue(ok);
        }
    }
}
