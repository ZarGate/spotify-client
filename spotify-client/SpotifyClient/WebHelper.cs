using System;
using System.Net;
using System.Text;
using System.Windows;
using Newtonsoft.Json;
using SpotifyClient.Properties;

namespace SpotifyClient
{
    internal static class WebHelper
    {
        public static void DoAsyncJsonRequest(string address, object data)
        {
            var payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            var client = new ExtendedWebClient();
            client.UploadDataCompleted += ClientUploadDataCompleted;
            client.UploadDataAsync(new Uri(address, UriKind.Absolute), payload);
        }

        public static T DoSyncJsonGetRequest<T>(string address)
        {
            var client = new ExtendedWebClient();
            var result = client.DownloadData(new Uri(address, UriKind.Absolute));
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(result));
        }

        private static void ClientUploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
            }
            else
            {
                if (Settings.Default.Debug)
                {
                    MessageBox.Show("Return from webrequest:" + Environment.NewLine + Encoding.UTF8.GetString(e.Result));
                }
            }
        }
    }

    internal class ExtendedWebClient : WebClient
    {
        public ExtendedWebClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12
                                                   | SecurityProtocolType.Ssl3;
            ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);
            Timeout = 3000;
            Headers.Add("Content-Type:application/json; charset=utf-8");
            Headers.Add("X-Custom-Authorization:" + Settings.Default.WebserviceSecret);
        }

        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
                request.Timeout = Timeout;
            return request;
        }
    }
}