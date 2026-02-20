using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AllKeyShopExtension.Utilities
{
    public class HttpClientHelper : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly object lockObject = new object();
        private DateTime lastRequestTime = DateTime.MinValue;
        private readonly TimeSpan minDelayBetweenRequests = TimeSpan.FromMilliseconds(500);

        public HttpClientHelper()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> GetStringAsync(string url)
        {
            await EnsureRateLimit();
            try
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.StatusCode} for {url}");
                }
            }
            catch (HttpRequestException)
            {
                throw; // Re-throw HTTP errors
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error requesting {url}: {ex.Message}", ex);
            }
        }

        private async Task EnsureRateLimit()
        {
            lock (lockObject)
            {
                var timeSinceLastRequest = DateTime.Now - lastRequestTime;
                if (timeSinceLastRequest < minDelayBetweenRequests)
                {
                    var delay = minDelayBetweenRequests - timeSinceLastRequest;
                    System.Threading.Thread.Sleep(delay);
                }
                lastRequestTime = DateTime.Now;
            }
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }
    }
}
