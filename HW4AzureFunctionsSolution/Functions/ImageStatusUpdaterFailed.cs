using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HW4AzureFunctionsSolution.Functions
{
    public static class ImageStatusUpdaterFailed
    {
        const string FAILED_IMAGES_ROUTE = "failedimages/{name}";

        /// <summary>
        /// HW:4 #4
        /// Function will be triggered when images are uploaded into the failedimages container.
        /// Updates the job table with the status of 4 as well as description of 
        /// the status as defined in the job status table definition. 
        /// Update the imageResult property with the Azure public url to the failed image
        /// </summary>
        /// <param name="cloudBlockBlob"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        [FunctionName("ImageStatusUpdaterFailed")]
        public static async Task Run([BlobTrigger(FAILED_IMAGES_ROUTE, Connection = ConfigurationSettings.STORAGE_CONNECTIONSTRING_NAME)] CloudBlockBlob cloudBlockBlob, string name, ILogger log)
        {
            // Get Job Id
            await cloudBlockBlob.FetchAttributesAsync();
            if (cloudBlockBlob.Metadata.ContainsKey(ConfigurationSettings.JOBID_METADATA_NAME))
            {
                string jobId = cloudBlockBlob.Metadata[ConfigurationSettings.JOBID_METADATA_NAME];
                
                string blobUri = cloudBlockBlob.Uri.ToString();
                log.LogInformation($"Blob URI: {blobUri}");

                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{cloudBlockBlob.Name} \n JobId: [{jobId}]");

                JobTable jobTable = new JobTable(log, ConfigurationSettings.IMAGEJOBS_PARTITIONKEY);
                await jobTable.UpdateJobEntityStatus(jobId, 4, "Oh no! Something has gone terribly wrong!", blobUri);
            }
            else
            {
                log.LogError($"The blob {cloudBlockBlob.Name} has no {ConfigurationSettings.JOBID_METADATA_NAME} metadata; can't update the job");
            }
        }
    }
}
