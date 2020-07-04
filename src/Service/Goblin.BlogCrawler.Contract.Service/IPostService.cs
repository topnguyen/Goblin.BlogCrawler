using System.Threading;
using System.Threading.Tasks;
using Goblin.BlogCrawler.Share.Models;
using Goblin.Core.Models;

namespace Goblin.BlogCrawler.Contract.Service
{
    public interface IPostService
    {
        Task<GoblinApiPagedResponseModel<GoblinBlogCrawlerPostModel>> GetPagedAsync(GoblinBlogCrawlerGetPagedPostModel model, CancellationToken cancellationToken = default);
        
        Task InitCrawlerJobAsync(CancellationToken cancellationToken = default);
    }
}