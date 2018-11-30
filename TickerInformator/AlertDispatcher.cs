using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace TickerInformator
{
    public static class AlertDispatcher
    {
        private static HttpClient httpClient = new HttpClient();

        //At 8 and 20
        [FunctionName("AlertDispatcherDaily")]
        public static async Task RunDaily([TimerTrigger("0 0 8,20 * * *")]TimerInfo myTimer,
         ILogger log,
         [Table("HourlyAlertLevels")] CloudTable subscribersTable,
         [Queue("tosendemail", Connection = "AzureWebJobsStorage")] IAsyncCollector<Alert> outputQueueItem)
        {
            log.LogInformation($"Timer trigger daily dispatcher at: {DateTime.Now}");
            await RunAlertDispatcher(log, subscribersTable, outputQueueItem, trackDailyChange: true);
        }

        //From 9 to 19, every hour
        [FunctionName("AlertDispatcherHourly")]
        public static async Task RunHourly([TimerTrigger("0 0 9-19 * * *")]TimerInfo myTimer,
         ILogger log,
         [Table("HourlyAlertLevels")] CloudTable subscribersTable,
         [Queue("tosendemail", Connection = "AzureWebJobsStorage")] IAsyncCollector<Alert> outputQueueItem)
        {
            log.LogInformation($"Timer trigger hourly dispatcher at: {DateTime.Now}");
            await RunAlertDispatcher(log, subscribersTable, outputQueueItem, trackDailyChange: false);
        }

        private static async Task RunAlertDispatcher(ILogger log, CloudTable subscribersTable, IAsyncCollector<Alert> outputQueueItem, bool trackDailyChange)
        {
            TickerHistory tickerHistory = await QueryTickerService(log);

            foreach (Datum data in tickerHistory.Data)
            {
                log.LogInformation($"USD to BTC at {data.Time.ToString()}: {data.Close}");
            }
            await SendAlertsToSubscribers(log, subscribersTable, outputQueueItem, trackDailyChange, tickerHistory);
        }

        private static async Task<TickerHistory> QueryTickerService(ILogger log)
        {
            HttpResponseMessage response = await httpClient.GetAsync(BuildTickerServiceUri());

            HttpContent responseContent = response.Content;
            using (StreamReader reader = new StreamReader(await responseContent.ReadAsStreamAsync()))
            {
                string result = await reader.ReadToEndAsync();
                TickerHistory history = JsonConvert.DeserializeObject<TickerHistory>(result);
                return history;
            }
        }

        private static async Task SendAlertsToSubscribers(ILogger log, CloudTable subscribersTable, IAsyncCollector<Alert> outputQueueItem, bool trackDailyChange, TickerHistory history)
        {
            decimal latest = history.Data[history.Data.Length - 1].Close;
            decimal lastHour = history.Data[history.Data.Length - 2].Close;
            decimal lastDay = history.Data[0].Close;
            decimal lastHourChange = (latest - lastHour) / lastHour;
            decimal lastDayChange = (latest - lastDay) / lastDay;

            log.LogInformation($"Last hour change: {decimal.Round(lastHourChange * 100, 2)}%");
            log.LogInformation($"Last day change: {decimal.Round(lastDayChange * 100, 2)}%");
            decimal trackedChange = trackDailyChange ? Math.Max(Math.Abs(lastHourChange), Math.Abs(lastDayChange)) : Math.Abs(lastDayChange);
            if (trackedChange >= 1)
            {
                foreach (var levelOfAlert in Enumerable.Range(1, (int)trackedChange))
                {
                    await SendAlertsForLevel(log, subscribersTable, outputQueueItem, lastHourChange, lastDayChange, levelOfAlert);
                }
            }
        }

        private static async Task SendAlertsForLevel(ILogger log, CloudTable subscribersTable, IAsyncCollector<Alert> outputQueueItem, decimal lastHourChange, decimal lastDayChange, int levelOfAlert)
        {
            List<TableEntity> subscribersForLevel = await subscribersTable.GetAll<TableEntity>(levelOfAlert.ToString("D3"));
            if (!subscribersForLevel.Any())
            {
                log.LogInformation($"No subscribers of level {levelOfAlert}");
            }
            foreach (TableEntity subscriber in subscribersForLevel)
            {
                await outputQueueItem.AddAsync(new Alert()
                {
                    Addressee = subscriber.RowKey,
                    LastHourChange = decimal.Round(lastHourChange * 100, 2),
                    LastDayChange = decimal.Round(lastDayChange * 100, 2)
                });
            }
        }

        private static string BuildTickerServiceUri()
        {
            var queryParameters = new Dictionary<string, string>();
            queryParameters["fsym"] = "BTC";
            queryParameters["tsym"] = "USD";
            queryParameters["limit"] = "24";
            queryParameters["aggregate"] = "1";
            queryParameters["e"] = "CCCAGG";
            queryParameters["api_key"] = Environment.GetEnvironmentVariable("minApiKey");

            return QueryHelpers.AddQueryString("https://min-api.cryptocompare.com/data/histohour", queryParameters);
        }
    }
}
