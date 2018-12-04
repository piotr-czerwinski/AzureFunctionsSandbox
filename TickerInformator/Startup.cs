using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TickerInformator;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

/**
 * Dependency injection
 * https://blog.wille-zone.de/post/dependency-injection-for-azure-functions/ 
 * 
 * Not working on .net core 2.1
 * https://github.com/Azure/azure-functions-host/issues/3386#issuecomment-419565714
 * 
 * Do not update WindowsAzure.Storage!
 * https://github.com/Azure/azure-functions-host/issues/3784
 */
[assembly: WebJobsStartup(typeof(Startup))]
namespace TickerInformator
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
            builder.AddDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ISubscriberService, SubscriberService>();
            services.AddSingleton<IEmailComposer, EmailComposer>();
            services.AddSingleton<IHTMLComposer, StaticStringHTMLComposer>();
        }
    }
}
