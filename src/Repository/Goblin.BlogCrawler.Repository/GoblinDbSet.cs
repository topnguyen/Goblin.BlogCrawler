using Goblin.BlogCrawler.Contract.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Goblin.BlogCrawler.Repository
{
    public sealed partial class GoblinDbContext
    {
        public DbSet<PostEntity> Posts { get; set; }
        
        public DbSet<SourceEntity> Sources { get; set; }
    }
}