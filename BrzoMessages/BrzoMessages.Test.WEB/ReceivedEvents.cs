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
            s = new Sync("cad587f6-4f06-4c9f-9575-ae500b5f161c", "DOvJHQ-CSB-tBs-u2HhE6RhwT2t6nZZ7");
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
