using System;
using System.Collections.Generic;
using System.Text;
using SendGrid.Helpers.Mail;

namespace TickerInformator
{
    public interface IEmailComposer
    {
        SendGridMessage CreateUpdateMessage(UpdaterOrchestratorData activationData);

        SendGridMessage CreateActivationMessage(UpdaterOrchestratorData activationData);

        SendGridMessage CreateAlertEmail(Alert alert);
    }
}
