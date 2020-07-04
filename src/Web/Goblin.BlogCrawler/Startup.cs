using Elect.Core.ConfigUtils;
using Goblin.Core.Web.Setup;
using Goblin.BlogCrawler.Core.Validators;
using Goblin.BlogCrawler.Core;
using Goblin.BlogCrawler.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Goblin.BlogCrawler
{
    public class Startup : BaseApiStartup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration) : base(env, configuration)
        {
            RegisterValidators.Add(typeof(IValidator));

            BeforeConfigureServices = services =>
            {
                // Setting

                SystemSetting.Current = Configuration.GetSection<SystemSetting>("Setting");
                
                // Database

                services.AddGoblinDbContext();
            };
        }
    }
}