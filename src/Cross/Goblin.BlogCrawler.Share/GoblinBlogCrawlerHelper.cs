using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Goblin.Core.Constants;
using Goblin.BlogCrawler.Share.Models;
using Goblin.Core.Errors;
using Goblin.Core.Models;

namespace Goblin.BlogCrawler.Share
{
    public static class GoblinBlogCrawlerHelper
    {
        public static string Domain { get; set; } = string.Empty;
        
        public static string AuthorizationKey { get; set; } = string.Empty;

        public static async Task<GoblinApiPagedMetaResponseModel<GoblinBlogCrawlerGetPagedPostModel, GoblinBlogCrawlerPostModel>> GetPagedAsync(GoblinBlogCrawlerGetPagedPostModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var endpoint = Domain
                    .WithHeader(GoblinHeaderKeys.Authorization, AuthorizationKey)
                    .AppendPathSegment(GoblinBlogCrawlerEndpoints.GetPagedPost);

                var userPagedMetaResponse = await endpoint
                    .PostJsonAsync(model, cancellationToken: cancellationToken)
                    .ReceiveJson<GoblinApiPagedMetaResponseModel<GoblinBlogCrawlerGetPagedPostModel, GoblinBlogCrawlerPostModel>>()
                    .ConfigureAwait(true);

                return userPagedMetaResponse;
            }
            catch (FlurlHttpException ex)
            {
                var goblinErrorModel = await ex.GetResponseJsonAsync<GoblinErrorModel>().ConfigureAwait(true);

                if (goblinErrorModel != null)
                {
                    throw new GoblinException(goblinErrorModel);
                }

                var responseString = await ex.GetResponseStringAsync().ConfigureAwait(true);

                var message = responseString ?? ex.Message;

                throw new Exception(message);
            }
        }

    }
}