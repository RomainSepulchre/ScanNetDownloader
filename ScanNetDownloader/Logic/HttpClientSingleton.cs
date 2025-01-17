using System.Net.Http;

namespace ScanNetDownloader.Logic
{
    internal class HttpClientSingleton
    {
        public static readonly HttpClient Client = new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
        });
    }
}
