using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using SendGrid.Helpers.Mail;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace TickerInformator
{
    public static class SubscriberDataUpdater
    {
        [FunctionName("SubscriberDataUpdaterOrchestrator")]
        public static async Task SubscriberDataUpdaterOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext ctx,
            ILogger log
            )
        {
            SubmitInfo submitInfo = ctx.GetInput<SubmitInfo>();
            await ctx.CallActivityAsync<string>("SendConfirmationEmail", new UpdaterOrchestratorData() { SubmitInfo = submitInfo, InstanceId = ctx.InstanceId });

            using (CancellationTokenSource timeoutCts = new CancellationTokenSource())
            {
                DateTime dueTime = ctx.CurrentUtcDateTime.AddHours(1);
                Task durableTimeout = ctx.CreateTimer(dueTime, timeoutCts.Token);
                Task<bool> approvalEvent = ctx.WaitForExternalEvent<bool>($"ApprovalEvent");
                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout)) //Terminate after 1 hour without confirmation event
                {
                    timeoutCts.Cancel();
                    bool updateResult = await ctx.CallActivityAsync<bool>("UpdateSubscriberInfo", submitInfo);
                    log.LogInformation($"Update finished for: {submitInfo.Email} with result {updateResult}");
                }
                else
                {
                    log.LogInformation($"Update timeout for: {submitInfo.Email}");
                }
            }
        }

        [FunctionName("SendConfirmationEmail")]
        [return: SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")]
        public static async Task<SendGridMessage> SendConfirmationEmail(
            [ActivityTrigger]UpdaterOrchestratorData orchestratorData,
            [Table("Subscribers")] CloudTable subscribersTable,
            [Inject] IEmailComposer emailComposer,
            ILogger log)
        {
            log.LogInformation($"SendConfirmationEmail to: {orchestratorData.SubmitInfo.Email}");

            string[] emailSplited = orchestratorData.SubmitInfo.Email.Split("@");
            if (emailSplited.Length != 2)
            {
                return null;
            }
            SubscriberInfo subscriberInfo = await subscribersTable.GetTableEntity<SubscriberInfo>(emailSplited[1], emailSplited[0]);
            return subscriberInfo == null ? emailComposer.CreateActivationMessage(orchestratorData) : emailComposer.CreateUpdateMessage(orchestratorData);
        }

        [FunctionName("ConfirmChange")]
        public static async Task<IActionResult> ConfirmChange(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequest req,
            [OrchestrationClient]DurableOrchestrationClient client,
            ILogger log)
        {
            string id = req.Query["id"];
            await client.RaiseEventAsync(id, "ApprovalEvent", true);
            return new OkObjectResult($"Thank you.");
        }

        [FunctionName("UpdateSubscriberInfo")]
        public static async Task<bool> UpdateSubscriberInfo(
            [ActivityTrigger]SubmitInfo submitInfo,
            [Table("Subscribers")] CloudTable subscribersTable,
            [Table("HourlyAlertLevels")] CloudTable alertLevelsTable,
            ILogger log)
        {
            //DI of tables to object not possible
            SubscriberService subscriberService = new SubscriberService(alertLevelsTable, subscribersTable);

            return await subscriberService.UpdateSubscriberData(submitInfo);
        }
    }
}