using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace BrzoMessages.Client
{
    internal class ConnectionStart : IDisposable
    {
        private readonly string keyAccess;
        private readonly string privateKey;
        private HttpClient client;

        internal ConnectionStart(string keyAccess, string privateKey)
        {
            this.keyAccess = keyAccess;
            this.privateKey = privateKey;

            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {privateKey}");
        }

        public void Dispose()
        {
            client.Dispose();
        }

        internal string Connect(string keyAccess)
        {
            try
            {
                var result = client.PostAsync($"{Config.CONNECT_URL}?token={keyAccess}", null).Result;
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return "";
                }
                return result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
