using Goblin.BlogCrawler.Contract.Repository.Models;

namespace Goblin.BlogCrawler.Contract.Repository.Interfaces
{
    public interface IGoblinUnitOfWork : Elect.Data.EF.Interfaces.UnitOfWork.IUnitOfWork
    {
        IGoblinRepository<T> GetRepository<T>() where T : GoblinEntity, new();
    }
}