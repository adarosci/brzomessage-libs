using BrzoMessages.Client.dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrzoMessages.Client.Interfaces
{
    public interface ISendMessage : IDisposable
    {
        (bool, int, string) Text(MessageText message, bool wait = true);
        (bool, int, string) Template(MessageTemplate message, bool wait = true);
        (bool, int, string) Image(MessageImage message, bool wait = true);
        (bool, int, string) Document(MessageDocument message, bool wait = true);
        (bool, int, string) Audio(MessageAudio message, bool wait = true);
        (bool, int, string) Video(MessageVideo message, bool wait = true);
    }
}
