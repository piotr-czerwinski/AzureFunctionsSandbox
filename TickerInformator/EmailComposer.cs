using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using SendGrid.Helpers.Mail;
using static TickerInformator.SubscriberDataUpdater;

namespace TickerInformator
{
    public static class EmailComposer
    {
        public static SendGridMessage CreateUpdateMessage(UpdaterOrchestratorData activationData)
        {
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Ticker informator update"
            };
            message.From = SenderAddress();
            message.AddTo(activationData.SubmitInfo.Email);
            message.AddContent("text/html", $"Update info: {GetConfirmationUri(activationData)}");
            return message;
        }

        public static SendGridMessage CreateActivationMessage(UpdaterOrchestratorData activationData)
        {
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Ticker informator activation"
            };
            message.From = SenderAddress();
            message.AddTo(activationData.SubmitInfo.Email);

            string confirmationUri = GetConfirmationUri(activationData);
            message.AddContent("text/html", $"Activate: {GetConfirmationUri(activationData)}");
            return message;
        }

        public static SendGridMessage CreateAlertEmail(Alert alert)
        {
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Ticker informator"
            };
            message.From = SenderAddress();
            message.AddTo(alert.Addressee);
            message.AddContent("text/html", $"Last day: {alert.LastDayChange}%, Last hour: {alert.LastHourChange}%");
            return message;
        }

        private static string GetConfirmationUri(UpdaterOrchestratorData activationData)
        {
            var queryParameters = new Dictionary<string, string>();
            queryParameters["id"] = activationData.InstanceId;
            return QueryHelpers.AddQueryString(Environment.GetEnvironmentVariable("HostUrl") + "/ConfirmChange", queryParameters);
        }

        private static EmailAddress SenderAddress()
        {
            return new EmailAddress(Environment.GetEnvironmentVariable("FromEmailAddress"), Environment.GetEnvironmentVariable("FromEmailName"));
        }
    }
}
