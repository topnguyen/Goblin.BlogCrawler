using Goblin.BlogCrawler.Contract.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Goblin.BlogCrawler.Repository.Maps
{
    public class SourceEntityMap : GoblinEntityMap<SourceEntity>
    {
        public override void Map(EntityTypeBuilder<SourceEntity> builder)
        {
            base.Map(builder);

            builder.ToTable(nameof(SourceEntity));

            builder.HasIndex(x => x.Name);
            
            builder.HasIndex(x => x.Url);
            
            builder.HasIndex(x => x.LastCrawledPostUrl);
        }
    }
}