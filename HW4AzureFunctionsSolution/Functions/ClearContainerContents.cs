using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HW4AzureFunctionsSolution.Functions
{
    public static class ClearContainerContents
    {
        /// <summary>
        /// HW:4 #Extra Credit
        /// Clears converttogreyscale and converttosepia containers avery 2 minutes
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        [FunctionName("ClearContainerContents")]
        public static async Task Run([TimerTrigger("0 * */2 * * *")] TimerInfo myTimer, ILogger log)
        {
            // Retrieve Storage Account
            string storageConnString = Environment.GetEnvironmentVariable(ConfigurationSettings.STORAGE_CONNECTIONSTRING_NAME);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnString);

            // Create a blob client for blobs to be retrieved and created
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve converttogreyscale container reference
            CloudBlobContainer converttogreyscaleContainer = blobClient.GetContainerReference(ConfigurationSettings.GRAYSCALEIMAGES_CONTAINERNAME);

            // delete blobs in grey scale container
            await deleteAllBlobItems(converttogreyscaleContainer, log);

            // Retrieve sepia container reference
            CloudBlobContainer converttosepiaContainer = blobClient.GetContainerReference(ConfigurationSettings.SEPIAIMAGES_CONTAINERNAME);

            // delete blobs in grey scale container
            await deleteAllBlobItems(converttosepiaContainer, log);

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        /// <summary>
        /// Deletes All blobs int the provided container reference
        /// </summary>
        /// <param name="container"></param>
        /// <param name="log"></param>
        private static async Task deleteAllBlobItems(CloudBlobContainer container, ILogger log)
        {
            log.LogInformation("Entered delete blob function");

            if (await container.ExistsAsync())
            {
                log.LogInformation($"Container {container} does infact exist");

                BlobResultSegment result = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.Metadata, null, null, new BlobRequestOptions(), new OperationContext());
                if (result?.Results != null)
                {
                    log.LogInformation("Results not null");

                    foreach (var blob in result.Results)
                    {
                        log.LogInformation($"Blob is {blob.ToString()}");

                        if (blob is CloudBlockBlob)
                        {
                            await container.GetBlockBlobReference(((CloudBlockBlob)(blob)).Name).DeleteAsync();
                            log.LogInformation($"Deleted {((CloudBlockBlob)(blob)).Name}: {DateTime.Now}");
                        }
                    }
                }
                log.LogInformation($"All Blobs in {container.Name}, deleted @{DateTime.Now}");
            }
            else
            {
                log.LogInformation($"container {container.Name} does not exist");
            }
            log.LogInformation("Exited delete method");
        }
    }
}
