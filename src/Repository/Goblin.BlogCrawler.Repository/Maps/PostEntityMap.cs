using Goblin.BlogCrawler.Contract.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Goblin.BlogCrawler.Repository.Maps
{
    public class PostEntityMap : GoblinEntityMap<PostEntity>
    {
        public override void Map(EntityTypeBuilder<PostEntity> builder)
        {
            base.Map(builder);

            builder.ToTable(nameof(PostEntity));

            builder.HasIndex(x => x.Title);
            
            builder.HasIndex(x => x.Url);
            
            builder.HasIndex(x => x.SiteName);
            
            builder.HasIndex(x => x.LastCrawledTime);
            
            builder.HasIndex(x => x.PublishTime);
        }
    }
}