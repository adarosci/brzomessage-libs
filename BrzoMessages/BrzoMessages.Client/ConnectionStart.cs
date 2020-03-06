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

        internal void Connect(string keyAccess)
        {
            try
            {
                var str = JsonConvert.SerializeObject(new { key_access = keyAccess });
                var result = client.PostAsync($"{Config.CONNECT_URL}/connect?token={keyAccess}", new StringContent(str, Encoding.UTF8, "application/json")).Result;
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Não foi possivel conectar");
                }
            }
            catch (Exception)
            {
                //throw;
            }
        }
    }
}
