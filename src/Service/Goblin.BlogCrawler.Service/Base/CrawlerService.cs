using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Repository.Models;
using Goblin.BlogCrawler.Service.PostCrawlers;
using Goblin.Core.DateTimeUtils;
using Microsoft.EntityFrameworkCore;

namespace Goblin.BlogCrawler.Service.Base
{
    public abstract class CrawlerService : Service
    {
        public string Name { get; set; }

        public string Domain { get; set; }

        public string StopAtPostUrl { get; set; }
        
        public string UrlPattern { get; set; }
        
        public string PostUrlQuerySelector { get; set; }
        
        public Dictionary<string, string> UrlPathDictionary { get; set; } = new Dictionary<string, string>();

        private readonly IGoblinRepository<SourceEntity> _sourceRepo;

        private readonly IGoblinRepository<PostEntity> _postRepo;

        protected CrawlerService(IGoblinUnitOfWork goblinUnitOfWork, IGoblinRepository<SourceEntity> sourceRepo, IGoblinRepository<PostEntity> postRepo) : base(goblinUnitOfWork)
        {
            _sourceRepo = sourceRepo;

            _postRepo = postRepo;
        }

        public virtual async Task CrawlPostsAsync(CancellationToken cancellationToken = default)
        {
            var startTime = GoblinDateTimeHelper.SystemTimeNow;

            var sourceEntity = await GetSourceEntity(cancellationToken);

            var crawledPostUrl = await CrawlPostUrlAsync(cancellationToken).ConfigureAwait(true);

            var postUrls = crawledPostUrl.TakeWhile(url => url != sourceEntity.LastCrawledPostUrl).ToList();

            var postsMetadata = await GoblinCrawlerHelper.GetListMetadataModelsAsync(postUrls).ConfigureAwait(true);

            using var transaction =
                await GoblinUnitOfWork.BeginTransactionAsync(cancellationToken).ConfigureAwait(true);

            // Posts Metadata to Post Crawled Database

            await GoblinCrawlerHelper.SavePostEntitiesAsync(postsMetadata, startTime, _postRepo, GoblinUnitOfWork)
                .ConfigureAwait(true);

            // Update Source

            sourceEntity.LastCrawlStartTime = startTime;
            sourceEntity.LastCrawlEndTime = GoblinDateTimeHelper.SystemTimeNow;
            sourceEntity.TimeSpent = sourceEntity.LastCrawlEndTime.Subtract(sourceEntity.LastCrawlStartTime);
            sourceEntity.TotalPostCrawledLastTime = postsMetadata.Count;
            sourceEntity.TotalPostCrawled += postsMetadata.Count;

            if (!string.IsNullOrWhiteSpace(postsMetadata.FirstOrDefault()?.Url))
            {
                sourceEntity.LastCrawledPostUrl = postsMetadata.FirstOrDefault()?.Url;
            }

            _sourceRepo.Update(sourceEntity,
                x => x.LastCrawlStartTime,
                x => x.LastCrawlEndTime,
                x => x.TimeSpent,
                x => x.TotalPostCrawledLastTime,
                x => x.TotalPostCrawled,
                x => x.LastCrawledPostUrl
            );

            await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);

            transaction.Commit();
        }

        protected async Task<SourceEntity> GetSourceEntity(CancellationToken cancellationToken)
        {
            Domain = Domain.Trim().Trim('/').ToLowerInvariant();
            
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

            StopAtPostUrl =
                sourceEntity.LastCrawledPostUrl
                    ?.Replace("http://", string.Empty)
                    .Replace("https://", string.Empty).Trim('/');

            return sourceEntity;
        }

        protected async Task<List<string>> CrawlPostUrlAsync(CancellationToken cancellationToken = default)
        {
            var urlPathDictionary = GetNextPageUrlDictionary();
            
            var postsUrlCrawled = await BasicCrawlPostUrlAsync(urlPathDictionary, cancellationToken).ConfigureAwait(true);

            return postsUrlCrawled;
        }

        protected async Task<List<string>> BasicCrawlPostUrlAsync(Dictionary<string, string> urlDictionary, CancellationToken cancellationToken = default)
        {
            using var browsingContext = GoblinCrawlerHelper.GetIBrowsingContext();

            var endpoint = GetEndpoint(urlDictionary);

            var htmlDocument = await browsingContext.OpenAsync(endpoint, cancellation: cancellationToken).ConfigureAwait(true);

            var postUrls = await GetPostUrlsAsync(browsingContext, htmlDocument).ConfigureAwait(true);

            postUrls = postUrls.Select(x => x.Trim().Trim('/').ToLowerInvariant()).ToList();

            var isStopCrawling = IsStopCrawling(postUrls);

            if (isStopCrawling)
            {
                return postUrls;
            }

            urlDictionary = GetNextPageUrlDictionary();

            var nextPagePostUrls = await BasicCrawlPostUrlAsync(urlDictionary, cancellationToken);

            postUrls.AddRange(nextPagePostUrls);

            return postUrls;
        }

        protected virtual string GetEndpoint(Dictionary<string, string> urlDictionary)
        {
            var endpoint = UrlPattern;

            foreach (var urlKey in urlDictionary.Keys)
            {
                endpoint = endpoint.Replace(urlKey, urlDictionary[urlKey]);
            }

            return endpoint;
        }

        protected virtual async Task<List<string>> GetPostUrlsAsync(IBrowsingContext browsingContext, IDocument htmlDocument)
        {
            await htmlDocument.WaitForReadyAsync().ConfigureAwait(true);
            
            var postUrls = htmlDocument
                .QuerySelectorAll(PostUrlQuerySelector)
                .OfType<IHtmlAnchorElement>()
                .Select(x => x.Href)
                .ToList();
            
            return postUrls;
        }

        protected bool IsContainStopAtPostUrl(IEnumerable<string> postUrls)
        {
            if (!string.IsNullOrWhiteSpace(StopAtPostUrl))
            {
                var postUrlsPath = postUrls.Select(x => x.Replace("http://", string.Empty).Replace("https://", string.Empty).Trim('/'));

                if (postUrlsPath.Contains(StopAtPostUrl))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        protected virtual bool IsStopCrawling(List<string> postUrls)
        {
            throw new NotImplementedException();
        }
        
        protected virtual Dictionary<string, string> GetNextPageUrlDictionary()
        {
            throw new NotImplementedException();
        }
    }
}