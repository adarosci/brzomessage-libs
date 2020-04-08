using BrzoMessages.Client;
using BrzoMessages.Client.dto;
using System;
using System.Threading;

namespace BrzoMessages.Test
{
    class Program
    {
        static void Main(string[] args)
        {            
            var s = new Sync("token", "privateKey");
            s.HandlerMessages += S_HandlerMessages;
            s.HandlerAck += S_HandlerAck;
            s.HandlerLogs += S_HandlerLogs;
            s.Connect();

            Thread.Sleep(Timeout.Infinite);
        }

        private static bool S_HandlerAck(MessageAck message)
        {
            Console.WriteLine(message.ID + " " + message.Ack);
            return true;
        }

        private static void S_HandlerLogs(string message)
        {
            Console.WriteLine(message);
        }

        private static bool S_HandlerMessages(MessageReceived message)
        {
            Console.WriteLine($"Message received: {DateTime.Now} - {message.data.Info.RemoteJid} - {message.data?.Text}");
            return true;
        }
    }
}
