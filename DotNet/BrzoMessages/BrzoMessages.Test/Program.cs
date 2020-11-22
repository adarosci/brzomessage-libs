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
            var t = new Thread(() =>
            {
                var s = new Sync("cad587f6-4f06-4c9f-9575-ae500b5f161c", "DOvJHQ-CSB-tBs-u2HhE6RhwT2t6nZZ7");
                s.HandlerMessages += S_HandlerMessages;
                s.HandlerAck += S_HandlerAck;
                s.HandlerLogs += S_HandlerLogs;
                s.HandlerJSON += S_HandlerJSON;
                s.Connect();
                
                //int i = 0;
                //while (true)
                //{
                //    Thread.Sleep(TimeSpan.FromSeconds(5));
                //    Console.WriteLine($"Conectado -> {s.IsConnected}");

                //    if (!s.IsConnected)
                //    {
                //        i++;
                //        if (i > 5)
                //        {
                //            s.Dispose();
                //        }
                //    }
                //}
            });
            t.Start();

            Thread.Sleep(Timeout.Infinite);


        }

        private static bool S_HandlerJSON(string message)
        {
            Console.WriteLine(message);
            return true;
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
