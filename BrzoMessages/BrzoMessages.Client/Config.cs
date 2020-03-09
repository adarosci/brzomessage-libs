using System;
using System.Collections.Generic;
using System.Text;

namespace BrzoMessages.Client
{
    internal class Config
    {
        public const string SOCKET_URL = "ws://localhost:3336/sync";
        public const string CONNECT_URL = "http://localhost:3336/connect";
        public const string AUTH_URL = "http://localhost:3336/auth";
        public const string DISCONNECT_URL = "http://localhost:3336/disconnect";
        public const string CONFIRM_MESSAGE_URL = "http://localhost:3336/confirm";
    }
}
