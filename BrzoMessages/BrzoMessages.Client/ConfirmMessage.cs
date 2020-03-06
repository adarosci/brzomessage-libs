using BrzoMessages.Client.dto;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace BrzoMessages.Client
{
    internal class ConfirmMessage : IDisposable
    {
        private string keyAccess;
        private string privateKey;
        private HttpClient client;

        internal ConfirmMessage(string keyAccess, string privateKey)
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

        internal void Ok(MessageReceived message)
        {
            try
            {
                var str = JsonConvert.SerializeObject(new { id = message.data.Info.Id });
                var result = client.PostAsync($"{Config.CONFIRM_MESSAGE_URL}/confirm?token={keyAccess}", new StringContent(str, Encoding.UTF8, "application/json")).Result;
                if (result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Não foi possivel confirmar mensagem");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
