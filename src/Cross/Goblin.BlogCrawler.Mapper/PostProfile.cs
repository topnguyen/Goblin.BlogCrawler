using AutoMapper;
using Elect.Mapper.AutoMapper.IMappingExpressionUtils;
using Goblin.BlogCrawler.Contract.Repository.Models;
using Goblin.BlogCrawler.Share.Models;

namespace Goblin.BlogCrawler.Mapper
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<PostEntity, GoblinBlogCrawlerPostModel>()
                .IgnoreAllNonExisting();
        }
    }
}