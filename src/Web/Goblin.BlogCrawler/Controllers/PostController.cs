using System.Threading;
using System.Threading.Tasks;
using Elect.Web.Swagger.Attributes;
using Goblin.BlogCrawler.Contract.Service;
using Goblin.BlogCrawler.Share;
using Goblin.BlogCrawler.Share.Models;
using Goblin.Core.Models;
using Goblin.Core.Web.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Goblin.BlogCrawler.Controllers
{
    public class PostController : BaseController
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        ///     Get Paged Post Crawled
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [ApiDocGroup("Post")]
        [HttpPost]
        [Route(GoblinBlogCrawlerEndpoints.GetPagedPost)]
        [SwaggerResponse(StatusCodes.Status200OK, "User Paged with Metadata",
            typeof(GoblinApiPagedMetaResponseModel<GoblinBlogCrawlerGetPagedPostModel, GoblinBlogCrawlerPostModel>))]
        public async Task<IActionResult> GetPaged([FromBody] GoblinBlogCrawlerGetPagedPostModel model,
            CancellationToken cancellationToken = default)
        {
            var pagedModel = await _postService.GetPagedAsync(model, cancellationToken);

            var pagedWithMetadataResponseModel = Url.GetGoblinApiPagedMetaResponseModel(model, pagedModel);

            return Ok(pagedWithMetadataResponseModel);
        }
    }
}