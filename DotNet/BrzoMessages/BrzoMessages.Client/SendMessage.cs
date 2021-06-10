using BrzoMessages.Client.dto;
using BrzoMessages.Client.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace BrzoMessages.Client
{
    public class SendMessage : ISendMessage
    {
        private HttpClient client;
        private Uri url;

        public SendMessage(string keyAccess, string privateKey)
        {
            client = new HttpClient();
            url = new Uri($"https://api.brzomessages.com/v1/messages/send?token={keyAccess}");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {privateKey}");
        }

        private string GetContent(object obj) => JsonConvert.SerializeObject(obj);
        private (bool, int, string) ErrorResult(string error) => (false, HttpStatusCode.InternalServerError.GetHashCode(), error);
        private (bool, int, string) ErrorResult(HttpStatusCode code, string error) => (false, code.GetHashCode(), error);
        private (bool, int, string) OkResult() => (true, HttpStatusCode.OK.GetHashCode(), null);
        private (bool, int, string) SendRequest(object message, bool wait = true)
        {
            try
            {
                var param = new StringContent(GetContent(message), Encoding.UTF8, "application/json");
                var uri = new Uri(url.ToString() + (wait ? "&wait=true" : ""));

                var result = client.PostAsync(uri, param).Result;

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return ErrorResult(result.StatusCode, result.Content.ReadAsStringAsync().Result);
                }

                return OkResult();
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message);
            }
        }

        public (bool, int, string) Text(MessageText message, bool wait = true) => SendRequest(message, wait);
        public (bool, int, string) Template(MessageTemplate message, bool wait = true) => SendRequest(message, wait);
        public (bool, int, string) Image(MessageImage message, bool wait = true) => SendRequest(message, wait);
        public (bool, int, string) Document(MessageDocument message, bool wait = true) => SendRequest(message, wait);
        public (bool, int, string) Audio(MessageAudio message, bool wait = true) => SendRequest(message, wait);
        public (bool, int, string) Video(MessageVideo message, bool wait = true) => SendRequest(message, wait);
        public void Dispose() => client.Dispose();
    }
}
