using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using SendGrid.Helpers.Mail;

namespace TickerInformator
{
    public static class SendAlertToSubscriber
    {
        [FunctionName("SendAlertToSubscriber")]
        [return: SendGrid(ApiKey = "CustomSendGridKeyAppSettingName")]
        public static SendGridMessage Run([QueueTrigger("tosendemail", Connection = "AzureWebJobsStorage")] Alert alert, ILogger log)
        {
            log.LogInformation($"Sending alert to: {alert.Addressee}");

            return EmailComposer.CreateAlertEmail(alert);
        }
    }
}
