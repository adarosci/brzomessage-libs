using System;
using System.Collections.Generic;
using System.Text;

namespace BrzoMessages.Client
{
    internal class Config
    {
        public const string SOCKET_URL = "ws://localhost:3336/sync";
        public const string CONNECT_URL = "ws://localhost:3333/sync";
        public const string DISCONNECT_URL = "ws://localhost:3333/sync";
        public const string CONFIRM_MESSAGE_URL = "ws://localhost:3333/sync";
    }
}
