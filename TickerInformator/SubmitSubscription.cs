using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TickerInformator
{
    public static class SubmitSubscription
    {
        [FunctionName("SubmitSubscription")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation(requestBody);

                SubmitInfo submitInfo = JsonConvert.DeserializeObject<SubmitInfo>(requestBody);
                log.LogInformation($"Subscription submited by {submitInfo.Email} with treshold {submitInfo.AlertThreshold}");
                if (!string.IsNullOrWhiteSpace(submitInfo.Email) && submitInfo.AlertThreshold.HasValue && submitInfo.AlertThreshold.Value >= 0)
                {
                    string instanceId = await starter.StartNewAsync("SubscriberDataUpdaterOrchestrator", submitInfo);
                    return new OkObjectResult($"Thank you, {submitInfo.Email}! Please wait for confirmation e-mail.");
                }
                else
                {
                    return new BadRequestObjectResult("Activation not sucessfull. Please pass valid email and threshold level");
                }
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.StackTrace);
                return new BadRequestObjectResult(ex.ToString());
            }
        }
    }
}
