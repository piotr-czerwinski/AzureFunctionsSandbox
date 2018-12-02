## Ticker info
Simple app to scan bitcoin price history and send e-mail alert to subscribers if gain or loss is greater than user defined threshold. Written purely with Azure Functions as learning playground. 


### Subscribtion form (HTTP triggered get and post)
`SubscribeForm` is GET function that returns simple form in HTML:

![Form](https://raw.githubusercontent.com/piotr-czerwinski/AzureFunctionsSandbox/master/doc/TickerInfoForm.PNG)

`SubmitSubscription` is POST function to handle request from above form. It also initiates orchestrator described below.

### Confirmation e-mail orchestrator
`SubscriberDataUpdaterOrchestrator`is DurableFunction with purpose of sending confirmation e-mail after subscription request. Confirmation must be done within 1 hour or orchestrator rejects. E-mails are send with SendGrid output binding. Confirmation e-mail contains link to `ConfirmChange` function, which raises event handled by orchestrator.

### Time triggered alert dispatcher (with Queue collector)
`AlertDispatcher.RunDaily`  and `AlertDispatcher.RunHourly` functions with cron expressions of `0 0 8,20 * * *` and `0 0 9-19 * * *"` respectively are responsible to query [cryptocompare api](https://min-api.cryptocompare.com/). If change in price is big enough dispatcher adds  to Azure storage Queue. 

### Queue triggered e-mail sender
`SendAlertToSubscribers` consumes queue item and sends e-mails via SendGrid:
![Alert](https://raw.githubusercontent.com/piotr-czerwinski/AzureFunctionsSandbox/master/doc/Alert.PNG)

### Dependency injection
I use [Willezone.Azure.WebJobs.Extensions.DependencyInjection] (https://www.nuget.org/packages/Willezone.Azure.WebJobs.Extensions.DependencyInjection) library which enable simple injection of dependeny with `[Inject]` attribute. Container configuration is performed on `Startup` class:

```
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
```
### Configuration
To start app locally configuration file local.settings.json should be added:
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "CustomSendGridKeyAppSettingName": "Send grid api key",
    "HostUrl": "api endpoint",
    "FromEmailAddress": "e-mail@of.alert.sender",
    "FromEmailName": "name of alert sender",
    "MinApiKey": "BTC service api key"
  }
}
```
