using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrzoMessages.Client.dto
{
    public class MessageAck
    {
        [JsonProperty("cmd")]
        public string Cmd { get; set; }
        
        [JsonProperty("id")]
        public string ID { get; set; }
        
        [JsonProperty("ack")]
        public int Ack { get; set; }
        
        [JsonProperty("from")]
        public string From { get; set; }
        
        [JsonProperty("to")]
        public string To { get; set; }
        
        [JsonProperty("t")]
        public int T { get; set; }
    }
}
