using BrzoMessages.Client;
using BrzoMessages.Client.dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BrzoMessages.Test.WEB
{
    public class ReceivedEvents
    {
        private Sync s;

        public ReceivedEvents()
        {
            s = new Sync("token", "privatekey");
            s.HandlerMessages += S_HandlerMessages;
            s.HandlerLogs += S_HandlerLogs;
            s.Connect();
        }

        private void S_HandlerLogs(string message)
        {
            Console.WriteLine(message);
        }

        private bool S_HandlerMessages(MessageReceived message)
        {
            Console.WriteLine($"Message received: {DateTime.Now} - {message.data.Info.RemoteJid} - {message.data?.Text}");
            return true;
        }
    }
}
