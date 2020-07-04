using Goblin.BlogCrawler.Contract.Repository.Models;

namespace Goblin.BlogCrawler.Contract.Repository.Interfaces
{
    public interface IGoblinRepository<T> : Elect.Data.EF.Interfaces.Repository.IBaseEntityRepository<T> where T : GoblinEntity, new()
    {
    }
}