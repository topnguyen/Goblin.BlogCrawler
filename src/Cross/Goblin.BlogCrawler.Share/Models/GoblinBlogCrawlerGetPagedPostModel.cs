using Goblin.Core.Models;

namespace Goblin.BlogCrawler.Share.Models
{
    public class GoblinBlogCrawlerGetPagedPostModel : GoblinApiPagedRequestModel
    {
        public string Term { get; set; }
    }
}