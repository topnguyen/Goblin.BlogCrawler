using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Io.Network;
using Elect.Core.CrawlerUtils;
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

        public static async Task<List<MetadataModel>> GetListMetadataModelsAsync(List<string> postUrls)
        {
            var postsMetadata = new List<MetadataModel>();

            if (postUrls.Any())
            {
                postUrls = postUrls.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
                
                var postUrlsArray = postUrls.ToArray();

                var postsMetadataTemp = await CrawlerHelper.GetListMetadataAsync(postUrlsArray).ConfigureAwait(true);

                foreach (var postMetadata in postsMetadataTemp)
                {
                    postMetadata.OriginalUrl = postMetadata.OriginalUrl?.Trim().Trim('/').ToLowerInvariant();
                    
                    postMetadata.Url = postMetadata.Url?.Trim().Trim('/').ToLowerInvariant();
                }
                
                // Only take Post have Title or Image

                postsMetadataTemp = postsMetadataTemp.Where(x => !string.IsNullOrWhiteSpace(x.Title) && !string.IsNullOrWhiteSpace(x.Image)).ToList();
                
                foreach (var postUrl in postUrls)
                {
                    var url = postUrl.Trim().Trim('/').ToLowerInvariant();

                    var postMetadata = postsMetadataTemp.FirstOrDefault(x => x.OriginalUrl == url || x.Url == url);

                    if (postMetadata != null)
                    {
                        postsMetadata.Add(postMetadata);
                    }
                }
            }

            return postsMetadata;
        }
        
        public static async Task SavePostEntitiesAsync(string sourceDomain, IEnumerable<MetadataModel> metadataModels, DateTimeOffset crawledTime, IGoblinRepository<PostEntity> postRepo, IGoblinUnitOfWork goblinUnitOfWork)
        {
            // Posts Metadata to Post Crawled Database
            foreach (var postMetadata in metadataModels)
            {
                var postEntity = new PostEntity
                {
                    SourceUrl = sourceDomain,
                    Url = postMetadata.OriginalUrl,
                    Title = postMetadata.Title,
                    ImageUrl = postMetadata.Image,
                    Description = postMetadata.Description,
                    SiteName = postMetadata.SiteName,
                    AuthorName = postMetadata.Author,
                    AuthorAvatarUrl = null,
                    PublishTime = postMetadata.PublishedTime == default ? crawledTime : postMetadata.PublishedTime,
                    LastCrawledTime = crawledTime
                };

                if (postMetadata.Tags?.Any() == true)
                {
                    postEntity.Tags = string.Join(",", postMetadata.Tags);
                }
                
                // Save to Database
                
                postRepo.Add(postEntity);

                await goblinUnitOfWork.SaveChangesAsync().ConfigureAwait(true);
            }
        }
    }
}