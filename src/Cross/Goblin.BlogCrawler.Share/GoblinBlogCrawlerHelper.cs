using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Goblin.Core.Constants;
using Goblin.BlogCrawler.Share.Models;
using Goblin.Core.Models;
using Goblin.Core.Settings;

namespace Goblin.BlogCrawler.Share
{
    public static class GoblinBlogCrawlerHelper
    {
        public static string Domain { get; set; } = string.Empty;

        public static string AuthorizationKey { get; set; } = string.Empty;

        public static readonly ISerializer JsonSerializer = new NewtonsoftJsonSerializer(GoblinJsonSetting.JsonSerializerSettings);

        private static IFlurlRequest GetRequest(long? loggedInUserId)
        {
            var request = Domain.WithHeader(GoblinHeaderKeys.Authorization, AuthorizationKey);

            if (loggedInUserId != null)
            {
                request = request.WithHeader(GoblinHeaderKeys.UserId, loggedInUserId);
            }

            request = request.ConfigureRequest(x =>
            {
                x.JsonSerializer = JsonSerializer;
            });

            return request;
        }

        public static async Task<GoblinApiPagedMetaResponseModel<GoblinBlogCrawlerGetPagedPostModel, GoblinBlogCrawlerPostModel>> GetPagedAsync(GoblinBlogCrawlerGetPagedPostModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var endpoint = GetRequest(null).AppendPathSegment(GoblinBlogCrawlerEndpoints.GetPagedPost);

                var userPagedMetaResponse = await endpoint
                    .PostJsonAsync(model, cancellationToken: cancellationToken)
                    .ReceiveJson<GoblinApiPagedMetaResponseModel<GoblinBlogCrawlerGetPagedPostModel, GoblinBlogCrawlerPostModel>>()
                    .ConfigureAwait(true);

                return userPagedMetaResponse;
            }
            catch (FlurlHttpException ex)
            {
                await FlurlHttpExceptionHelper.HandleErrorAsync(ex).ConfigureAwait(true);

                return null;
            }
        }
    }
}