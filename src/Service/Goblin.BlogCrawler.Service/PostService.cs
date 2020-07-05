﻿using System.Linq;
using System.Linq.Dynamic.Core;
using Elect.DI.Attributes;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Service;
using System.Threading;
using System.Threading.Tasks;
using Elect.Core.EnvUtils;
using Elect.Mapper.AutoMapper.IQueryableUtils;
using Goblin.BlogCrawler.Contract.Repository.Models;
using Goblin.BlogCrawler.Service.PostCrawlers;
using Goblin.BlogCrawler.Share.Models;
using Goblin.Core.Models;
using Goblin.Core.Utils;
using Hangfire;

namespace Goblin.BlogCrawler.Service
{
    [ScopedDependency(ServiceType = typeof(IPostService))]
    public class PostService : Base.Service, IPostService
    {
        private readonly IGoblinRepository<PostEntity> _postRepo;
        private readonly ICrawlerService<BlogCwaMeUkCrawlerService> _blogCwaMeUkCrawlerService;
        private readonly ICrawlerService<DotNetWeeklyCrawlerService> _dotNetWeeklyCrawlerService;

        public PostService(IGoblinUnitOfWork goblinUnitOfWork, 
            IGoblinRepository<PostEntity> postRepo,
            ICrawlerService<BlogCwaMeUkCrawlerService> blogCwaMeUkCrawlerService,
            ICrawlerService<DotNetWeeklyCrawlerService> dotNetWeeklyCrawlerService
        ) : base(
            goblinUnitOfWork)
        {
            _postRepo = postRepo;
            _blogCwaMeUkCrawlerService = blogCwaMeUkCrawlerService;
            _dotNetWeeklyCrawlerService = dotNetWeeklyCrawlerService;
        }

        public Task<GoblinApiPagedResponseModel<GoblinBlogCrawlerPostModel>> GetPagedAsync(
            GoblinBlogCrawlerGetPagedPostModel model, CancellationToken cancellationToken = default)
        {
            var query = _postRepo.Get();

            // Filters

            if (model.Term != null)
            {
                query = query.Where(x => x.Title.Contains(model.Term) ||
                                         x.Description.Contains(model.Term) ||
                                         x.Tags.Contains(model.Term) ||
                                         x.AuthorName.Contains(model.Term) ||
                                         x.SiteName.Contains(model.Term)
                );
            }

            // Excludes

            var excludeIds = model.ExcludeIds.ToList<long>(long.TryParse);

            if (excludeIds.Any())
            {
                query = query.Where(x => !excludeIds.Contains(x.Id));
            }

            var includeIds = model.IncludeIds.ToList<long>(long.TryParse);

            // Includes

            if (includeIds.Any())
            {
                var includeIdsQuery = _postRepo.Get(x => includeIds.Contains(x.Id));

                query = query.Union(includeIdsQuery);
            }

            // Build Model Query

            var modelQuery = query.QueryTo<GoblinBlogCrawlerPostModel>();

            // Order

            var sortByDirection = model.IsSortAscending ? "ASC" : "DESC";

            var orderByDynamicStatement = $"{model.SortBy} {sortByDirection}";

            modelQuery = !string.IsNullOrWhiteSpace(model.SortBy)
                ? modelQuery.OrderBy(orderByDynamicStatement)
                : modelQuery.OrderByDescending(x => x.LastCrawledTime);

            // Get Result

            var total = query.LongCount();

            var items =
                modelQuery
                    .Skip(model.Skip)
                    .Take(model.Take)
                    .ToList();

            var pagedResponse = new GoblinApiPagedResponseModel<GoblinBlogCrawlerPostModel>
            {
                Total = total,
                Items = items
            };

            return Task.FromResult(pagedResponse);
        }

        public Task InitCrawlerJobAsync(CancellationToken cancellationToken = default)
        {
            var cronTime3HoursPerTime = "3 */3 * * *";
            
            var cronTime7HoursPerTime = "7 */7 * * *";

            if (EnvHelper.IsDevelopment())
            {
                cronTime3HoursPerTime = Cron.Never();
                cronTime7HoursPerTime = Cron.Never();
            }

            RecurringJob.RemoveIfExists(nameof(BlogCwaMeUkCrawlerService));
            RecurringJob.AddOrUpdate(nameof(BlogCwaMeUkCrawlerService), () => _blogCwaMeUkCrawlerService.CrawlPostsAsync(CancellationToken.None), cronTime3HoursPerTime);
            
            RecurringJob.RemoveIfExists(nameof(DotNetWeeklyCrawlerService));
            RecurringJob.AddOrUpdate(nameof(DotNetWeeklyCrawlerService), () => _dotNetWeeklyCrawlerService.CrawlPostsAsync(CancellationToken.None), cronTime7HoursPerTime);

            return Task.CompletedTask;
        }
    }
}