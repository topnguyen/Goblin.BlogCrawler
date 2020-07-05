using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using Elect.DI.Attributes;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Repository.Models;
using Goblin.BlogCrawler.Contract.Service;
using Goblin.Core.DateTimeUtils;
using Microsoft.EntityFrameworkCore;

namespace Goblin.BlogCrawler.Service.PostCrawlers
{
    [ScopedDependency(ServiceType = typeof(ICrawlerService<HaackedCrawlerService>))]
    public class HaackedCrawlerService : Base.Service, ICrawlerService<HaackedCrawlerService>
    {
        public string Name { get; } = "You've Been Haacked | Phil Haack";

        public string Domain { get; } = "https://haacked.com";

        private readonly IGoblinRepository<SourceEntity> _sourceRepo;
        private readonly IGoblinRepository<PostEntity> _postRepo;

        public HaackedCrawlerService(IGoblinUnitOfWork goblinUnitOfWork,
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

            var postUrls = postUrlsTemp.TakeWhile(url => url != sourceEntity.LastCrawledPostUrl).ToList();

            var postsMetadata = await GoblinCrawlerHelper.GetListMetadataModelsAsync(postUrls).ConfigureAwait(true);

            using var transaction = await GoblinUnitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(true);

            // Posts Metadata to Post Crawled Database
            
            await GoblinCrawlerHelper.SavePostEntitiesAsync(postsMetadata, startTime, _postRepo, GoblinUnitOfWork).ConfigureAwait(true);

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

            var endpoint = $"{Domain}/archive/page/{pageNo}";

            if (pageNo == 1)
            {
                endpoint = Domain;
            }

            var htmlDocument = await browsingContext.OpenAsync(endpoint, cancellation: cancellationToken)
                .ConfigureAwait(true);

            var postUrls = htmlDocument
                .QuerySelectorAll(".post-title a")
                .OfType<IHtmlAnchorElement>()
                .Select(x => x.Href)
                .ToList();

            if (!string.IsNullOrWhiteSpace(stopAtPostUrl))
            {
                var stopAtPostUrlPath = stopAtPostUrl
                    .Replace("http://", string.Empty)
                    .Replace("https://", string.Empty)
                    .Trim('/');

                var postUrlsPath =
                    postUrls
                        .Select(x => 
                            x.Replace("http://", string.Empty)
                                .Replace("https://", string.Empty)
                                .Trim('/'));

                if (postUrlsPath.Contains(stopAtPostUrlPath))
                {
                    return postUrls;
                }
            }

            if (pageNo == 20)
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