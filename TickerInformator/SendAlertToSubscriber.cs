using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using SendGrid.Helpers.Mail;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace TickerInformator
{
    public static class SendAlertToSubscriber
    {
        [FunctionName("SendAlertToSubscriber")]
        [return: SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")]
        public static SendGridMessage Run([QueueTrigger("tosendemail", Connection = "AzureWebJobsStorage")] Alert alert,
            [Inject] IEmailComposer emailComposer,
            ILogger log)
        {
            log.LogInformation($"Sending alert to: {alert.Addressee}");

            return emailComposer.CreateAlertEmail(alert);
        }
    }
}
