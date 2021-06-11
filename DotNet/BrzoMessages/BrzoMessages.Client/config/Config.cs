using System;
using System.Collections.Generic;
using System.Text;

namespace BrzoMessages.Client
{
    public class Config
    {
        public string SOCKET_URL = "wss://socket.{url}/sync";
        public string CONNECT_URL = "https://socket.{url}/connect";
        public string AUTH_URL = "https://socket.{url}/auth";
        public string DISCONNECT_URL = "https://socket.{url}/disconnect";
        public string CONFIRM_MESSAGE_URL = "https://socket.{url}/confirm";
        public string NATS_URL = "nats://nats.{url}";
        public string MAGANER_URL = "https://manager.{url}";

        public static Config NewConfig(string baseUrl = null)
        {
            if (baseUrl == null)
            {
                baseUrl = "brzomessages.com";
            }
            
            var c = new Config();

            c.SOCKET_URL = c.SOCKET_URL.Replace("{url}", baseUrl);
            c.CONNECT_URL = c.CONNECT_URL.Replace("{url}", baseUrl);
            c.AUTH_URL = c.AUTH_URL.Replace("{url}", baseUrl);
            c.DISCONNECT_URL = c.DISCONNECT_URL.Replace("{url}", baseUrl);
            c.CONFIRM_MESSAGE_URL = c.CONFIRM_MESSAGE_URL.Replace("{url}", baseUrl);
            c.NATS_URL = c.NATS_URL.Replace("{url}", baseUrl);
            c.MAGANER_URL = c.MAGANER_URL.Replace("{url}", baseUrl);

            return c;
        }
    }


    //internal class Config
    //{
    //    public const string NATS_URL = "nats://127.0.0.1:4222";
    //    public const string SOCKET_URL = "ws://localhost:3336/sync";
    //    public const string CONNECT_URL = "http://localhost:3336/connect";
    //    public const string AUTH_URL = "http://localhost:3336/auth";
    //    public const string DISCONNECT_URL = "http://localhost:3336/disconnect";
    //    public const string CONFIRM_MESSAGE_URL = "http://localhost:3336/confirm";
    //}
}
