using System;

namespace Goblin.BlogCrawler.Share.Models
{
    public class GoblinBlogCrawlerPostModel
    {
        public long Id { get; set; }

        // Post Info
        
        public string Url { get; set; }
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public string ImageUrl { get; set; }
        
        public string Tags { get; set; }
        
        // Author
        
        public string SiteName { get; set; }

        public string AuthorName { get; set; }
        
        public string AuthorAvatarUrl { get; set; }

        // Time
        
        public DateTimeOffset PublishTime { get; set; }

        public DateTimeOffset LastCrawledTime { get; set; }
    }
}