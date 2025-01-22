using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HW4AzureFunctionsSolution
{
    class JobTable
    {
        private CloudTableClient _tableClient;
        private CloudTable _table;
        private string _partitionKey;

        public JobTable(ILogger log, string partitionKey)
        {
            string storageConnectionString = Environment.GetEnvironmentVariable(ConfigurationSettings.STORAGE_CONNECTIONSTRING_NAME);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the table client
            _tableClient = storageAccount.CreateCloudTableClient();

            // Create cloud table object representing jobentity table
            _table = _tableClient.GetTableReference(ConfigurationSettings.JOBS_TABLENAME);

            _table.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            _partitionKey = partitionKey;
        }

        /// <summary>
        /// Retrieves the job entity
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns>JobEntity</returns>
        public async Task<JobEntity> RetrieveJobEntity(string jobId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<JobEntity>(_partitionKey, jobId);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

            return retrievedResult.Result as JobEntity;
        }

        /// <summary>
        /// Returns All Jobs for a provided Partition Key
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public async Task<List<JobEntity>> RetrieveAllJobs()
        {
            List<JobEntity> jobs = new List<JobEntity>();

            TableQuery<JobEntity> query = new TableQuery<JobEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey));

            var continuationToken = default(TableContinuationToken);

            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = queryResult.ContinuationToken;
                if (queryResult.Results != null)
                {
                    foreach (JobEntity jobEntity in queryResult.Results)
                    {
                        jobs.Add(jobEntity);
                    }
                }
            } while (continuationToken != null);


            return jobs;
        }

        /// <summary>
        /// Updates the job entity
        /// </summary>
        /// <param name="jobEntity"></param>
        public async Task<bool> UpdateJobEntity(JobEntity jobEntity)
        {
            TableOperation replaceOperation = TableOperation.Replace(jobEntity);
            TableResult result = await _table.ExecuteAsync(replaceOperation);

            return (result.HttpStatusCode > 199 && result.HttpStatusCode < 300) ? true : false;
        }

        /// <summary>
        /// Updates job entity with status
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task UpdateJobEntityStatus(string jobId, int status, string message, string blobUri)
        {
            JobEntity jobEntityToReplace = await RetrieveJobEntity(jobId);
            if (jobEntityToReplace != null)
            {
                jobEntityToReplace.JobId = jobId;
                jobEntityToReplace.Status = status;
                jobEntityToReplace.StatusDescription = message;
                jobEntityToReplace.ImageSource = blobUri;
                await UpdateJobEntity(jobEntityToReplace);
            }
        }

        /// <summary>
        /// Inserts or replaces job entity
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task InsertOrReplaceJobEntity(string jobId, int status, string message, string conversionMode, string blobUri)
        {
            JobEntity jobEntityToInsertOrReplace = new JobEntity();
            jobEntityToInsertOrReplace.JobId = jobId;
            jobEntityToInsertOrReplace.RowKey = jobId + "" + _partitionKey;
            jobEntityToInsertOrReplace.PartitionKey = _partitionKey;
            jobEntityToInsertOrReplace.Status = status;
            jobEntityToInsertOrReplace.StatusDescription = message;
            jobEntityToInsertOrReplace.ImageConversionMode = conversionMode;
            jobEntityToInsertOrReplace.ImageSource = blobUri;

            TableOperation insertReplaceOp = TableOperation.InsertOrReplace(jobEntityToInsertOrReplace);
            //TableResult tableResult = await _table.ExecuteAsync(insertReplaceOp);
            await _table.ExecuteAsync(insertReplaceOp);
        }
    }
}
