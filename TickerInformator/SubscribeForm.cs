using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace TickerInformator
{
    public static class SubscribeForm
    {
        [FunctionName("SubscribeForm")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [Inject] IHTMLComposer HTMLComposer,
            ILogger log)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(HTMLComposer.BuildSubscribeFormHTML());
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }

    }
}
