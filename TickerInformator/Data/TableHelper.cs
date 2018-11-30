using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Table
{
    public static class CloudTableExtensions
    {
        public static async Task<TEntity> GetTableEntity<TEntity>(this CloudTable cloudTable, string partitionKey, string rowKey)
            where TEntity : ITableEntity, new()
        {
            TableQuery<TEntity> rangeQuery = new TableQuery<TEntity>().Where(
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey)));

            List<TEntity> entities = new List<TEntity>();
            foreach (TEntity entity in await cloudTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                entities.Add(entity);
            }

            return entities.FirstOrDefault();
        }

        public static async Task<List<TEntity>> GetAll<TEntity>(this CloudTable cloudTable, string partitionKey = null)
            where TEntity : ITableEntity, new()
        {
            TableQuery<TEntity> rangeQuery = null;
            if (!string.IsNullOrEmpty(partitionKey))
            {
                rangeQuery = new TableQuery<TEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            }
            else
            {
                rangeQuery = new TableQuery<TEntity>();
            }

            List<TEntity> entities = new List<TEntity>();
            foreach (TEntity entity in await cloudTable.ExecuteQuerySegmentedAsync(rangeQuery, null))
            {
                entities.Add(entity);
            }

            return entities;
        }
    }
}