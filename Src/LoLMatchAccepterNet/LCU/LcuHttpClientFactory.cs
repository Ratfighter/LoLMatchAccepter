using System.Text;

namespace LoLMatchAccepterNet.LCU
{
    public class LcuHttpClientFactory
    {
        public HttpClient CreateClient(LcuConnectionInfo connectionInfo)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new HttpClient(handler);

            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{connectionInfo.Password}"));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");

            return client;
        }
    }
}
