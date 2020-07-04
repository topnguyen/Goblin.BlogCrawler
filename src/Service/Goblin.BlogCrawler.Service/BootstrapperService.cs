using Elect.DI.Attributes;
using Goblin.BlogCrawler.Contract.Repository.Interfaces;
using Goblin.BlogCrawler.Contract.Service;
using System.Threading;
using System.Threading.Tasks;

namespace Goblin.BlogCrawler.Service
{
    [ScopedDependency(ServiceType = typeof(IBootstrapperService))]
    public class BootstrapperService : Base.Service, IBootstrapperService
    {
        private readonly IPostService _postService;
        private readonly IBootstrapper _bootstrapper;

        public BootstrapperService(IGoblinUnitOfWork goblinUnitOfWork, IPostService postService, IBootstrapper bootstrapper) : base(goblinUnitOfWork)
        {
            _postService = postService;
            _bootstrapper = bootstrapper;
        }

        public async Task InitialAsync(CancellationToken cancellationToken = default)
        {
            await _bootstrapper.InitialAsync(cancellationToken).ConfigureAwait(true);

            await _postService.InitCrawlerJobAsync(cancellationToken);
        }

        public async Task RebuildAsync(CancellationToken cancellationToken = default)
        {
            await _bootstrapper.RebuildAsync(cancellationToken).ConfigureAwait(true);
        }
    }
}