using System.Net.Http;
using AngleSharp;
using AngleSharp.Io.Network;

namespace Goblin.BlogCrawler.Service.PostCrawlers
{
    public static class GoblinCrawlerHelper
    {
        public static IBrowsingContext GetIBrowsingContext()
        {
            var httpClient = new HttpClient();
            
            // Re-procedure as Native browser
                
            httpClient
                .DefaultRequestHeaders
                .Add("user-agent",
                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36");

            httpClient
                .DefaultRequestHeaders
                .Add("accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");

            httpClient
                .DefaultRequestHeaders
                .Add("upgrade-insecure-requests", "1");

            var requester = new HttpClientRequester(httpClient);

            var browsingConfig = Configuration
                .Default
                .WithRequester(requester)
                .WithDefaultCookies()
                .WithDefaultLoader();

            return BrowsingContext.New(browsingConfig);
        }
    }
}