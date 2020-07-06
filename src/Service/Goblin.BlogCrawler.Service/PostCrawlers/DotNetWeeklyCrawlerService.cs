using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dasync.Collections;
using Elect.Core.DictionaryUtils;
using Elect.DI.Attributes;
using Flurl.Util;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Repository.Models;
using Goblin.BlogCrawler.Contract.Service;
using Goblin.Core.DateTimeUtils;

namespace Goblin.BlogCrawler.Service.PostCrawlers
{
    [ScopedDependency(ServiceType = typeof(ICrawlerService<DotNetWeeklyCrawlerService>))]
    public class DotNetWeeklyCrawlerService : Base.CrawlerService, ICrawlerService<DotNetWeeklyCrawlerService>
    {
        private int _weekNow;

        private int _weekCurr;

        private int _yearCurr;

        public DotNetWeeklyCrawlerService(IGoblinUnitOfWork goblinUnitOfWork, IGoblinRepository<SourceEntity> sourceRepo, IGoblinRepository<PostEntity> postRepo) : base(goblinUnitOfWork, sourceRepo, postRepo)
        {
            Name = "dotNET Weekly";
            
            Domain = "https://www.dotnetweekly.com";

            UrlPattern = $"{Domain}/week/{{week}}/year/{{year}}";

            PostUrlQuerySelector = ".post-title a";
            
            var dateTimeNow = GoblinDateTimeHelper.SystemTimeNow;

            _weekNow = _weekCurr = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTimeNow.DateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            _yearCurr = dateTimeNow.Year;
        }
        
        protected override bool IsStopCrawling(List<string> postUrls)
        {
            var isContainStopAtPostUrl = IsContainStopAtPostUrl(postUrls);

            if (isContainStopAtPostUrl)
            {
                return true;
            }
            
            var diffWeek = _weekNow - _weekCurr;

            // not get posts belong to older 8 weeks (2 months)
            
            if (diffWeek > 8)
            {
                return true;
            }

            return false;
        }

        protected override Dictionary<string, string> GetNextPageUrlDictionary()
        {
            if (UrlPathDictionary?.Any() != true)
            {
                UrlPathDictionary = new Dictionary<string, string>
                {
                    {"{week}", _weekCurr.ToInvariantString()},
                    {"{year}", _yearCurr.ToInvariantString()}
                };

                return UrlPathDictionary;
            }

            if (UrlPathDictionary.TryGetValue("{week}", out var currentWeekNoStr))
            {
                var currentWeekNo = long.Parse(currentWeekNoStr);

                if (currentWeekNo == 1)
                {
                    _weekCurr = 52;
                
                    _yearCurr--;
                
                    _weekNow += 52;
                }
                else
                {
                    _weekCurr--;
                }
            }
            
            UrlPathDictionary.AddOrUpdate("{week}", _weekCurr.ToInvariantString());
            
            UrlPathDictionary.AddOrUpdate("{year}", _yearCurr.ToInvariantString());
            
            return UrlPathDictionary;
        }

        protected override async Task<List<string>> GetPostUrlsAsync(IBrowsingContext browsingContext, IDocument htmlDocument)
        {            
            await htmlDocument.WaitForReadyAsync().ConfigureAwait(true);

            var postUrlsInDotNetWeekly = htmlDocument
                .QuerySelectorAll("div.link a")
                .OfType<IHtmlAnchorElement>()
                .Select(x => x.Href.Trim('/'))
                .ToList();

            postUrlsInDotNetWeekly = postUrlsInDotNetWeekly.Distinct().ToList();
         
            var concurrentBag =  new ConcurrentBag<KeyValuePair<string, string>>();
            
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
                    postUrlInDotnetWeekly = postUrlInDotnetWeekly.Trim('/');
                    
                    actualPostUrl = actualPostUrl.Trim('/');
                    
                    concurrentBag.Add(new KeyValuePair<string, string>(postUrlInDotnetWeekly, actualPostUrl));
                }
                
            }, maxDegreeOfParallelism: 100);

            var postLinkedUrls = concurrentBag.ToList().ToDictionary(x => x.Key, x => x.Value);
            
            var postUrls = new List<string>();

            foreach (var postUrlInDotNetWeekly in postUrlsInDotNetWeekly)
            {
                if (postLinkedUrls.TryGetValue(postUrlInDotNetWeekly, out var postUrl))
                {
                    postUrls.Add(postUrl);
                }
            }

            return postUrls;
        }
    }
}