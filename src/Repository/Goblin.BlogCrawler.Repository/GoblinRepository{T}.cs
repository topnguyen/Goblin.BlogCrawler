using Elect.DI.Attributes;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Repository.Models;

namespace Goblin.BlogCrawler.Repository
{
    [ScopedDependency(ServiceType = typeof(IGoblinRepository<>))]
    public class GoblinRepository<T> : Elect.Data.EF.Services.Repository.BaseEntityRepository<T>, IGoblinRepository<T> where T : GoblinEntity, new()
    {
        public GoblinRepository(Elect.Data.EF.Interfaces.DbContext.IDbContext dbContext) : base(dbContext)
        {
        }
    }
}