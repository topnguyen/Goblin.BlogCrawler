using System.Threading;
using System.Threading.Tasks;

namespace Goblin.BlogCrawler.Contract.Service
{
    public interface ICrawlerService<T> where T : class
    {
        string Name { get; }
        
        string Domain { get; }
        
        Task CrawlPostsAsync(CancellationToken cancellationToken = default);
    }
}