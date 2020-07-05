using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Io.Network;
using Elect.Core.CrawlerUtils.Models;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Repository.Models;

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

        public static async Task<List<PostEntity>> GetAndSavePostEntitiesAsync(List<MetadataModel> metadataModels, DateTimeOffset crawledTime, IGoblinRepository<PostEntity> postRepo, IGoblinUnitOfWork goblinUnitOfWork)
        {
            var postEntities = new List<PostEntity>();
            
            // Posts Metadata to Post Crawled Database
            foreach (var postMetadata in metadataModels)
            {
                var postEntity = new PostEntity
                {
                    Url = postMetadata.Url,
                    Title = postMetadata.Title,
                    ImageUrl = postMetadata.Image,
                    Tags = string.Empty,
                    SiteName = postMetadata.SiteName,
                    AuthorName = postMetadata.Author,
                    AuthorAvatarUrl = null,
                    PublishTime = crawledTime,
                    LastCrawledTime = crawledTime
                };

                // Handle Publish Time

                var publishedTimeMetaTag =
                    postMetadata.MetaTags.FirstOrDefault(x =>
                        x.Attributes.Any(y => y.Value.Contains("published_time")));

                if (publishedTimeMetaTag != null)
                {
                    if (publishedTimeMetaTag.Attributes.TryGetValue("content", out var content))
                    {
                        if (DateTimeOffset.TryParse(content, out var publishedTime))
                        {
                            postEntity.PublishTime = publishedTime;
                        }
                    }
                }

                // Handle Tag

                var tagsMetaTags = postMetadata.MetaTags.Where(x => x.Attributes.Any(y => y.Value.Contains("tag")))
                    .ToList();

                if (tagsMetaTags.Any())
                {
                    foreach (var tagsMetaTag in tagsMetaTags)
                    {
                        if (tagsMetaTag.Attributes.TryGetValue("content", out var content))
                        {
                            postEntity.Tags += $",{content}";
                        }
                    }

                    postEntity.Tags = postEntity.Tags.Trim(',');
                }
                
                // Save to Database
                
                postRepo.Add(postEntity);

                await goblinUnitOfWork.SaveChangesAsync().ConfigureAwait(true);

                // Add to Result
                
                postEntities.Add(postEntity);
            }

            return postEntities;
        }
    }
}