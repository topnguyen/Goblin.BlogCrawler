using System.Collections.Generic;
using System.Linq;
using Elect.Core.DictionaryUtils;
using Elect.DI.Attributes;
using Flurl.Util;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Repository.Models;
using Goblin.BlogCrawler.Contract.Service;

namespace Goblin.BlogCrawler.Service.PostCrawlers
{
    [ScopedDependency(ServiceType = typeof(ICrawlerService<TalkingDotNetCrawlerService>))]
    public class TalkingDotNetCrawlerService : Base.CrawlerService, ICrawlerService<TalkingDotNetCrawlerService>
    {
        public TalkingDotNetCrawlerService(IGoblinUnitOfWork goblinUnitOfWork, IGoblinRepository<SourceEntity> sourceRepo, IGoblinRepository<PostEntity> postRepo) : base(goblinUnitOfWork, sourceRepo, postRepo)
        {
            Name = "Talking Dotnet";
            
            Domain = "https://www.talkingdotnet.com";

            UrlPattern = $"{Domain}/page/{{pageNo}}";

            PostUrlQuerySelector = ".entry-title a";
        }

        protected override bool IsStopCrawling(List<string> postUrls)
        {
            var isContainStopAtPostUrl = IsContainStopAtPostUrl(postUrls);

            if (isContainStopAtPostUrl)
            {
                return true;
            }

            if (!UrlPathDictionary.TryGetValue("{pageNo}", out var currentPageNoStr))
            {
                return false;
            }
            
            var currentPageNo = long.Parse(currentPageNoStr);

            return currentPageNo >= 20;
        }

        protected override Dictionary<string, string> GetNextPageUrlDictionary()
        {
            if (UrlPathDictionary?.Any() != true)
            {
                UrlPathDictionary = new Dictionary<string, string> {{"{pageNo}", "1"}};

                return UrlPathDictionary;
            }

            if (UrlPathDictionary.TryGetValue("{pageNo}", out var currentPageNoStr))
            {
                var currentPageNo = long.Parse(currentPageNoStr);

                currentPageNo++;
                
                UrlPathDictionary.AddOrUpdate("{pageNo}", currentPageNo.ToInvariantString());
            }
            else
            {
                UrlPathDictionary.AddOrUpdate("{pageNo}", "1");
            }
            
            return UrlPathDictionary;
        }
    }
}