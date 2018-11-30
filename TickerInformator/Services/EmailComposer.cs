using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using SendGrid.Helpers.Mail;

namespace TickerInformator
{
    public class EmailComposer : IEmailComposer
    {
        EmailAddress _sender;
        public EmailComposer()
        {
            _sender = new EmailAddress(Environment.GetEnvironmentVariable("FromEmailAddress"), Environment.GetEnvironmentVariable("FromEmailName"));
        }
        public SendGridMessage CreateUpdateMessage(UpdaterOrchestratorData activationData)
        {
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Ticker informator update"
            };
            message.From = _sender;
            message.AddTo(activationData.SubmitInfo.Email);
            message.AddContent("text/html", $"Update info: {GetConfirmationUri(activationData)}");
            return message;
        }

        public SendGridMessage CreateActivationMessage(UpdaterOrchestratorData activationData)
        {
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Ticker informator activation"
            };
            message.From = _sender;
            message.AddTo(activationData.SubmitInfo.Email);

            string confirmationUri = GetConfirmationUri(activationData);
            message.AddContent("text/html", $"Activate: {GetConfirmationUri(activationData)}");
            return message;
        }

        public SendGridMessage CreateAlertEmail(Alert alert)
        {
            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Ticker informator"
            };
            message.From = _sender;
            message.AddTo(alert.Addressee);
            message.AddContent("text/html", $"Current price: {alert.CurrentPrice} USD, last day: {alert.LastDayChange}%, last hour: {alert.LastHourChange}%");
            return message;
        }

        private static string GetConfirmationUri(UpdaterOrchestratorData activationData)
        {
            var queryParameters = new Dictionary<string, string>();
            queryParameters["id"] = activationData.InstanceId;
            return QueryHelpers.AddQueryString(Environment.GetEnvironmentVariable("HostUrl") + "/ConfirmChange", queryParameters);
        }
    }
}
