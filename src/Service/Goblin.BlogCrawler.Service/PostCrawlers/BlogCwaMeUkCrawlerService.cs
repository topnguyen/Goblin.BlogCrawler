using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using Elect.Core.CrawlerUtils;
using Elect.Core.CrawlerUtils.Models;
using Elect.DI.Attributes;
using Flurl;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Repository.Models;
using Goblin.BlogCrawler.Contract.Service;
using Goblin.Core.DateTimeUtils;
using Microsoft.EntityFrameworkCore;

namespace Goblin.BlogCrawler.Service.PostCrawlers
{
    [ScopedDependency(ServiceType = typeof(ICrawlerService<BlogCwaMeUkCrawlerService>))]
    public class BlogCwaMeUkCrawlerService : Base.Service, ICrawlerService<BlogCwaMeUkCrawlerService>
    {
        public string Name { get; } = "The Morning Brew";
        
        public string Domain { get; } = "http://blog.cwa.me.uk/";

        private readonly IGoblinRepository<SourceEntity> _sourceRepo;
        private readonly IGoblinRepository<PostEntity> _postRepo;

        public BlogCwaMeUkCrawlerService(IGoblinUnitOfWork goblinUnitOfWork,
            IGoblinRepository<SourceEntity> sourceRepo,
            IGoblinRepository<PostEntity> postRepo
            ) 
            : base(goblinUnitOfWork)
        {
            _sourceRepo = sourceRepo;
            _postRepo = postRepo;
        }

        public async Task CrawlPostsAsync(CancellationToken cancellationToken = default)
        {
            var startTime = GoblinDateTimeHelper.SystemTimeNow;

            var sourceEntity = await _sourceRepo
                .Get(x => x.Url == Domain)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(true);

            if (sourceEntity == null)
            {
                sourceEntity = new SourceEntity
                {
                    Name = Name,
                    Url = Domain,
                    TimeSpent = TimeSpan.Zero,
                    LastCrawledPostUrl = null
                };

                _sourceRepo.Add(sourceEntity);

                await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
            }
            
            var postUrlsTemp = await GetPostUrlAsync(1, sourceEntity.LastCrawledPostUrl, cancellationToken).ConfigureAwait(true);

            var postUrls = new List<string>();
            
            foreach (var url in postUrlsTemp)
            {
                if (url == sourceEntity.LastCrawledPostUrl)
                {
                    break;
                }
                
                postUrls.Add(url);
            }

            var postsMetadata = new List<MetadataModel>();
            
            if (postUrls.Any())
            {
                var postUrlsArray = postUrls.Distinct().ToArray();
                
                postsMetadata = await CrawlerHelper.GetListMetadataAsync(postUrlsArray).ConfigureAwait(true);
            }

            using var transaction = await GoblinUnitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(true);
            
            // Posts Metadata to Post Crawled Database
            foreach (var postMetadata in postsMetadata)
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
                    PublishTime = startTime,
                    LastCrawledTime = startTime
                };

                // Handle Publish Time
                
                var publishedTimeMetaTag = postMetadata.MetaTags.FirstOrDefault(x => x.Attributes.Any(y => y.Value.Contains("published_time")));

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
                
                var tagsMetaTags = postMetadata.MetaTags.Where(x => x.Attributes.Any(y => y.Value.Contains("tag"))).ToList();

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
                
                _postRepo.Add(postEntity);
                
                await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
            }

            // Update Source
            
            sourceEntity.LastCrawlStartTime = startTime;
            sourceEntity.LastCrawlEndTime = GoblinDateTimeHelper.SystemTimeNow;
            sourceEntity.TimeSpent = sourceEntity.LastCrawlEndTime.Subtract(sourceEntity.LastCrawlStartTime);
            sourceEntity.TotalPostCrawled = postsMetadata.Count;

            if (!string.IsNullOrWhiteSpace(postsMetadata.FirstOrDefault()?.Url))
            {
                sourceEntity.LastCrawledPostUrl = postsMetadata.FirstOrDefault()?.Url;
            }
            
            _sourceRepo.Update(sourceEntity,
                x => x.LastCrawlStartTime,
                x => x.LastCrawlEndTime,
                x => x.TimeSpent,
                x => x.TotalPostCrawled,
                x => x.LastCrawledPostUrl
                );

            await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
            
            transaction.Commit();
        }

        private async Task<List<string>> GetPostUrlAsync(int pageNo, string stopAtPostUrl, CancellationToken cancellationToken = default)
        {
            using var browsingContext = GoblinCrawlerHelper.GetIBrowsingContext();

            var endpoint = Domain.AppendPathSegment($"page/{pageNo}");
            
            var htmlDocument = await browsingContext.OpenAsync(endpoint, cancellation: cancellationToken).ConfigureAwait(true);

            var postUrls = htmlDocument
                .QuerySelectorAll("div.post-content li a")
                .OfType<IHtmlAnchorElement>()
                .Select(x => x.Href)
                .ToList();

            if (string.IsNullOrWhiteSpace(stopAtPostUrl))
            {
                if (pageNo == 20)
                {
                    return postUrls;
                }
                
                pageNo++;

                var nextPagePostUrls = await GetPostUrlAsync(pageNo, stopAtPostUrl, cancellationToken);

                postUrls.AddRange(nextPagePostUrls);
                
                return postUrls;
            }
            else
            {
                if (postUrls.Contains(stopAtPostUrl))
                {
                    return postUrls;
                }
                
                pageNo++;

                var nextPagePostUrls = await GetPostUrlAsync(pageNo, stopAtPostUrl, cancellationToken);
                    
                postUrls.AddRange(nextPagePostUrls);

                return postUrls;
            }
        }
    }
}