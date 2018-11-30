using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace TickerInformator
{
    public class SubscriberService : ISubscriberService
    {
        private readonly CloudTable _alertsTable;
        private readonly CloudTable _subscribersTable;

        public SubscriberService(CloudTable alertsTable, CloudTable subscribersTable)
        {
            _alertsTable = alertsTable;
            _subscribersTable = subscribersTable;
        }

        public async Task<bool> UpdateSubscriberData(SubmitInfo submitInfo)
        {
            string[] emailSplited = submitInfo.Email.Split("@");
            SubscriberInfo existingInfo = await _subscribersTable.GetTableEntity<SubscriberInfo>(emailSplited[1], emailSplited[0]);

            SubscriberInfo newSubscriberInfo = new SubscriberInfo()
            {
                PartitionKey = emailSplited[1],
                RowKey = emailSplited[0],
                Active = true,
                AlertTreshold = submitInfo.AlertTreshold ?? 0
            };
            if (existingInfo != null)
            {
                newSubscriberInfo.ETag = existingInfo.ETag;
            }

            TableOperation subscribersTableOperation = null;
            if (existingInfo != null)
            {
                if ((submitInfo.AlertTreshold ?? 0) == 0)
                {
                    await DeleteAlertLevelsOperation(existingInfo.AlertTreshold, submitInfo.Email);
                    subscribersTableOperation = TableOperation.Delete(existingInfo);

                }
                else if (submitInfo.AlertTreshold != existingInfo.AlertTreshold)
                {
                    await DeleteAlertLevelsOperation(existingInfo.AlertTreshold, submitInfo.Email);
                    await InsertAlertLevelsOperation(submitInfo.AlertTreshold.Value, submitInfo.Email);
                    subscribersTableOperation = TableOperation.Replace(newSubscriberInfo);

                }
            }
            else
            {
                await InsertAlertLevelsOperation(submitInfo.AlertTreshold.Value, submitInfo.Email);
                subscribersTableOperation = TableOperation.Insert(newSubscriberInfo);
            }

            if (subscribersTableOperation != null)
            {
                await _subscribersTable.ExecuteAsync(subscribersTableOperation);
                return true;
            }
            return false;
        }

        private async Task<bool> InsertAlertLevelsOperation(int newLevel, string rowKey)
        {
            if (newLevel <= 0)
            {
                return false;
            }
            TableEntity entityToBeInserted = new TableEntity()
            {
                PartitionKey = newLevel.ToString("D3"),
                RowKey = rowKey,
                ETag = "*" //conflicts are of no concern
            };
            await _alertsTable.ExecuteAsync(TableOperation.InsertOrReplace(entityToBeInserted));
            return true;
        }

        private async Task<bool> DeleteAlertLevelsOperation(int levelToBeDeleted, string rowKey)
        {
            if (levelToBeDeleted <= 0)
            {
                return false;
            }
            TableEntity entityToBeDeleted = new TableEntity()
            {
                PartitionKey = levelToBeDeleted.ToString("D3"),
                RowKey = rowKey,
                ETag = "*" //conflicts are of no concern
            };
            try
            {
                await _alertsTable.ExecuteAsync(TableOperation.Delete(entityToBeDeleted));
            }
            catch (StorageException storageException) //when entity does not exesit already it throws
            {
                return false;
            }
            return true;
        }
    }
}
