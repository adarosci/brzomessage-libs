using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BrzoMessages.Client.dto
{
    public abstract class MessageHeader
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("destiny")]
        public List<string> Destiny { get; set; }
    }
    public class MessageText : MessageHeader
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Template
    {
        [JsonProperty("template_name")]
        public string TemplateName { get; set; }
    }

    public class MessageTemplate : MessageHeader
    {

        [JsonProperty("template")]
        public Template Template { get; set; }

        [JsonProperty("parameters")]
        public List<string> Parameters { get; set; }
    }

    public class Image
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }

    public class MessageImage : MessageText
    {
        [JsonProperty("image")]
        public Image Image { get; set; }
    }

    public class Document
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("page_count")]
        public int PageCount { get; set; }
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class MessageDocument : MessageHeader
    {
        [JsonProperty("document")]
        public Document Document { get; set; }
    }

    public class Audio
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class MessageAudio : MessageHeader
    {
        [JsonProperty("audio")]
        public Audio Audio { get; set; }
    }

    public class Video
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }
    }

    public class MessageVideo : MessageText
    {
        [JsonProperty("video")]
        public Video Video { get; set; }
    }
}
