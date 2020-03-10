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
            var s = new BrzoSync("cad587f6-4f06-4c9f-9575-ae500b5f161c", "DOvJHQ-CSB-tBs-u2HhE6RhwT2t6nZZ73");
            s.HandlerMessages += S_HandlerMessages;
            s.HandlerLogs += S_HandlerLogs;
            s.Connect();

            Thread.Sleep(Timeout.Infinite);
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
