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
            var s = new Sync("554196584089", "aaaaaaaaaaa");

            s.EventConsoleOnCancelKeyPress += S_EventConsoleOnCancelKeyPress;
            s.EventCurrentDomainOnProcessExit += S_EventCurrentDomainOnProcessExit;
            s.EventDefaultOnUnloading += S_EventDefaultOnUnloading;
            s.EventErrorConnectionOrMessageProcessed += S_EventErrorConnectionOrMessageProcessed;

            s.Connect(metodo);

            Thread.Sleep(Timeout.Infinite);
        }

        private static void S_EventErrorConnectionOrMessageProcessed(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }

        private static void S_EventDefaultOnUnloading(System.Runtime.Loader.AssemblyLoadContext obj)
        {
            Console.WriteLine(obj.ToString());
        }

        private static void S_EventCurrentDomainOnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine(sender.ToString());
        }

        private static bool metodo(MessageReceived arg)
        {
            Console.WriteLine(arg.data.Text);
            return true;
        }

        private static void S_EventConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine(sender.ToString());
        }
    }
}
