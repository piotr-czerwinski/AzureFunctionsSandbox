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
 */
//[assembly: WebJobsStartup(typeof(Startup))]
namespace TickerInformator
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
            builder.AddDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            //services.
            //services.AddTransient<ITransientGreeter, Greeter>();
            //services.AddScoped<IScopedGreeter, Greeter>();
            //services.AddSingleton<ISingletonGreeter, Greeter>();
        }
    }
}
