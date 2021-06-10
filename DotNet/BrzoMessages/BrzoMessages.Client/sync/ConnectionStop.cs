using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace BrzoMessages.Client
{
    internal class ConnectionStop : IDisposable
    {
        private readonly string keyAccess;
        private readonly string privateKey;
        private HttpClient client;

        internal ConnectionStop(string keyAccess, string privateKey)
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

        internal void Disconnect(string accessKey, string auth)
        {
            try
            {
                var result = client.PostAsync($"{Config.DISCONNECT_URL}?token={keyAccess}&auth={auth}", null).Result;
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Não foi possivel conectar");
                }
            }
            catch (Exception)
            {
                
            }
        }
    }
}
