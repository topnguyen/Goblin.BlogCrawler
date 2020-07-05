using System;

namespace Goblin.BlogCrawler.Contract.Repository.Models
{
    public class SourceEntity : GoblinEntity
    {
        public string Name { get; set; }
        
        public string Url { get; set; }

        public string LastCrawledPostUrl { get; set; }
        
        public DateTimeOffset LastCrawlStartTime { get; set; }
        
        public DateTimeOffset LastCrawlEndTime { get; set; }

        public long TotalPostCrawledLastTime { get; set; }
        
        public long TotalPostCrawled { get; set; }
        
        public TimeSpan TimeSpent { get; set; }
    }
}