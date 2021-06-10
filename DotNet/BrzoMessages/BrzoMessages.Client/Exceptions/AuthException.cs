using System;
using System.Collections.Generic;
using System.Text;

namespace BrzoMessages.Client.Exceptions
{
    internal class AuthException : Exception
    {
        public AuthException(string message) : base(message)
        {
        }
    }
}
