using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HW4AzureFunctionsSolution.Functions
{
    public static class ImageStatusUpdaterSuccess
    {
        const string CONVERTED_IMAGES_ROUTE = "convertedimages/{name}";

        /// <summary>
        /// HW: 4 #3
        /// Triggered  when images are uploaded into the convertedimages container.
        /// Update the job with the status and description.
        /// </summary>
        /// <param name="cloudBlockBlob"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        [FunctionName("ImageStatusUpdaterSuccess")]
        public static async Task Run([BlobTrigger(CONVERTED_IMAGES_ROUTE, Connection = ConfigurationSettings.STORAGE_CONNECTIONSTRING_NAME)] CloudBlockBlob cloudBlockBlob, string name, ILogger log)
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
                await jobTable.UpdateJobEntityStatus(jobId, 3, "Success! We are cooking with-grease!", blobUri);
            }
            else
            {
                log.LogError($"The blob {cloudBlockBlob.Name} has no {ConfigurationSettings.JOBID_METADATA_NAME} metadata; can't update the job");
            }
        }
    }
}
