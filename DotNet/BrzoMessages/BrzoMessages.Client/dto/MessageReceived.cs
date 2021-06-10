namespace BrzoMessages.Client.dto
{
    public class Info
    {
        public string Id { get; set; }
        public string RemoteJid { get; set; }
        public string SenderJid { get; set; }
        public bool FromMe { get; set; }
        public int Timestamp { get; set; }
        public string PushName { get; set; }
        public int Status { get; set; }
    }

    public class ContextInfo
    {
        public string QuotedMessageID { get; set; }
        public object QuotedMessage { get; set; }
        public string Participant { get; set; }
        public bool IsForwarded { get; set; }
    }

    public class Data
    {
        public Info Info { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public string Caption { get; set; }
        public int Length { get; set; }
        public ContextInfo ContextInfo { get; set; }
    }

    public class MessageReceived
    {
        public string type { get; set; }
        public Data data { get; set; }
        public string file { get; set; }
        public string wid { get; set; }
    }

    internal class Body
    {
        public string Data { get; set; }
    }

    internal class MessageReceivedSocket
    {
        public string id { get; set; }
        public int version { get; set; }
        public Body body { get; set; }
    }
}
