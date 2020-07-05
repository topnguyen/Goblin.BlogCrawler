using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dasync.Collections;
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
    [ScopedDependency(ServiceType = typeof(ICrawlerService<DotNetWeeklyCrawlerService>))]
    public class DotNetWeeklyCrawlerService : Base.Service, ICrawlerService<DotNetWeeklyCrawlerService>
    {
        public string Name { get; } = "dotNET Weekly";

        public string Domain { get; } = "https://www.dotnetweekly.com/";

        private int _weekNow;

        private int _yearNow;

        private readonly IGoblinRepository<SourceEntity> _sourceRepo;
        private readonly IGoblinRepository<PostEntity> _postRepo;

        public DotNetWeeklyCrawlerService(IGoblinUnitOfWork goblinUnitOfWork,
            IGoblinRepository<SourceEntity> sourceRepo,
            IGoblinRepository<PostEntity> postRepo
        )
            : base(goblinUnitOfWork)
        {
            _sourceRepo = sourceRepo;
            _postRepo = postRepo;

            var dateTimeNow = GoblinDateTimeHelper.SystemTimeNow;

            _weekNow = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTimeNow.DateTime,
                CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            _yearNow = dateTimeNow.Year;
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

            var postUrlsTemp =
                    await GetPostUrlAsync(_weekNow, _yearNow, sourceEntity.LastCrawledPostUrl, cancellationToken)
                    .ConfigureAwait(true);

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
            
            await GoblinCrawlerHelper.GetAndSavePostEntitiesAsync(postsMetadata, startTime, _postRepo, GoblinUnitOfWork).ConfigureAwait(true);
            
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

        private async Task<List<string>> GetPostUrlAsync(int week, int year, string stopAtPostUrl,
            CancellationToken cancellationToken = default)
        {
            using var browsingContext = GoblinCrawlerHelper.GetIBrowsingContext();

            var endpoint = Domain.AppendPathSegment($"week/{week}/year/{year}");

            var htmlDocument = await browsingContext.OpenAsync(endpoint, cancellation: cancellationToken)
                .ConfigureAwait(true);

            var postUrls = await GetPostUrlsAsync(browsingContext, htmlDocument).ConfigureAwait(true);

            if (!string.IsNullOrWhiteSpace(stopAtPostUrl) && postUrls.Contains(stopAtPostUrl))
            {
                return postUrls;
            }

            if (week == 1)
            {
                week = 52;
                
                year--;
                
                _weekNow += 52;
            }
            else
            {
                week--;
            }

            int diffWeek = _weekNow - week;

            // not get posts belong to older 8 weeks (2 months)
            if (diffWeek > 8)
            {
                return postUrls;
            }

            var nextPagePostUrls = await GetPostUrlAsync(week, year, stopAtPostUrl, cancellationToken);

            postUrls.AddRange(nextPagePostUrls);

            return postUrls;
        }

        private static async Task<List<string>> GetPostUrlsAsync(IBrowsingContext browsingContext, IDocument htmlDocument)
        {            
            var postUrlsInDotNetWeekly = htmlDocument
                .QuerySelectorAll("div.link a")
                .OfType<IHtmlAnchorElement>()
                .Select(x => x.Href)
                .ToList();
         
            var postUrlsConcurrentBag =  new ConcurrentBag<string>();
            
            await postUrlsInDotNetWeekly.ParallelForEachAsync(async postUrlInDotnetWeekly =>
            {
                var postDetailHtmlDocument = await browsingContext.OpenAsync(postUrlInDotnetWeekly)
                    .ConfigureAwait(true);

                var actualPostUrl = postDetailHtmlDocument
                    .QuerySelectorAll("a.button")
                    .OfType<IHtmlAnchorElement>()
                    .Select(x => x.Href)
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(actualPostUrl))
                {
                    postUrlsConcurrentBag.Add(actualPostUrl);
                }
                
            }, maxDegreeOfParallelism: 100);

            var postUrls = postUrlsConcurrentBag.ToList();

            return postUrls;
        }
    }
}