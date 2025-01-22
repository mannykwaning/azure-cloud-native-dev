using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HW4AzureFunctionsSolution
{
    public static class ImageConsumerSepia
    {
        const string ImagesToConvertRoute = "converttosepia/{name}";

        /// <summary>
        /// HW:4 #2 
        /// Converts uploaded images in the converttosepia blob container to sepia
        /// if success --> send converted image to convertedimages container
        /// if fail    --> send image to failedimages container
        /// ----------------
        /// Records are added to the jobs table indicating job statuses
        /// </summary>
        /// <param name="cloudBlockBlob"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ImageConsumerSepia")]
        public static async Task Run([BlobTrigger(ImagesToConvertRoute,
            Connection = ConfigurationSettings.STORAGE_CONNECTIONSTRING_NAME)]CloudBlockBlob cloudBlockBlob,
            string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n ContentType: {cloudBlockBlob.Properties.ContentType} Bytes");

            using (Stream blobStream = await cloudBlockBlob.OpenReadAsync())
            {
                // Retrieve Storage Account
                string storageConnString = Environment.GetEnvironmentVariable(ConfigurationSettings.STORAGE_CONNECTIONSTRING_NAME);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnString);

                // Create a blob client for blobs to be retrieved and created
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Create or retrieve converted images container reference
                CloudBlobContainer convertedImagesContainer = blobClient.GetContainerReference(ConfigurationSettings.CONVERTED_IMAGES_CONTAINERNAME);
                bool created = await convertedImagesContainer.CreateIfNotExistsAsync();
                log.LogInformation($"[{ConfigurationSettings.CONVERTED_IMAGES_CONTAINERNAME}] Container needed to be created: {created}");

                CloudBlobContainer failedImagesContainer = blobClient.GetContainerReference(ConfigurationSettings.FAILED_IMAGES_CONTAINERNAME);
                created = await failedImagesContainer.CreateIfNotExistsAsync();
                log.LogInformation($"[{ConfigurationSettings.FAILED_IMAGES_CONTAINERNAME}] Container was created: {created}");

                string uploadedBlobUri = cloudBlockBlob.Uri.ToString();
                log.LogInformation($"Uploaded Image Uri: {uploadedBlobUri}");

                await ConvertAndStoreImage(log, blobStream, convertedImagesContainer, name, failedImagesContainer, uploadedBlobUri);
            }
        }

        /// <summary>
        /// Converts the image and stores is in converted 
        /// images or failed images container
        /// </summary>
        /// <param name="log"></param>
        /// <param name="uploadedImage"></param>
        /// <param name="convertedImagesContainer"></param>
        /// <param name="blobName"></param>
        /// <param name="failedImagesContainer"></param>
        private static async Task ConvertAndStoreImage(ILogger log,
                                    Stream uploadedImage,
                                    CloudBlobContainer convertedImagesContainer,
                                    string blobName,
                                    CloudBlobContainer failedImagesContainer,
                                    string uploadedBlobUri)
        {
            string convertedBlobName = $"{Guid.NewGuid()}--{blobName}";
            string jobId = Guid.NewGuid().ToString();

            try
            {
                await UpdateJobTableWithStatus(log, jobId, status: 2, message: "Image Being Converted", conversionMode: "Sepia", uploadedBlobUri);

                uploadedImage.Seek(0, SeekOrigin.Begin);

                using (MemoryStream convertedMemoryStream = new MemoryStream())
                using (Image<Rgba32> image = (Image<Rgba32>)Image.Load(uploadedImage))
                {
                    log.LogInformation($"[+] Starting conversion of image {blobName}");

                    image.Mutate(x => x.Sepia());
                    image.SaveAsJpeg(convertedMemoryStream);

                    convertedMemoryStream.Seek(0, SeekOrigin.Begin);
                    log.LogInformation($"[-] Completed conversion of image {blobName}");

                    log.LogInformation($"[+] Storing converted image {blobName} into {ConfigurationSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                    CloudBlockBlob convertedBlockBlob = convertedImagesContainer.GetBlockBlobReference(convertedBlobName);

                    convertedBlockBlob.Metadata.Add(ConfigurationSettings.JOBID_METADATA_NAME, jobId);
                    convertedBlockBlob.Properties.ContentType = MediaTypeNames.Image.Jpeg;
                    await convertedBlockBlob.UploadFromStreamAsync(convertedMemoryStream);

                    log.LogInformation($"[-] Stored converted image {convertedBlobName} into {ConfigurationSettings.CONVERTED_IMAGES_CONTAINERNAME} container");

                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to convert blob {blobName} Exception ex {ex.Message}");
                await StoreFailedImage(log, uploadedImage, blobName, failedImagesContainer, convertedBlobName: convertedBlobName, jobId: jobId);
            }
        }

        /// <summary>
        /// Stores failed Images
        /// </summary>
        /// <param name="log"></param>
        /// <param name="uploadedImage"></param>
        /// <param name="blobName"></param>
        /// <param name="failedImagesContainer"></param>
        /// <param name="convertedBlobName"></param>
        /// <param name="jobId"></param>
        private static async Task StoreFailedImage(ILogger log, Stream uploadedImage, string blobName, CloudBlobContainer failedImagesContainer, string convertedBlobName, string jobId)
        {
            try
            {
                log.LogInformation($"[+] Storing failed image {blobName} into {ConfigurationSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");

                CloudBlockBlob failedBlockBlob = failedImagesContainer.GetBlockBlobReference(convertedBlobName);
                failedBlockBlob.Metadata.Add(ConfigurationSettings.JOBID_METADATA_NAME, jobId);

                uploadedImage.Seek(0, SeekOrigin.Begin);
                await failedBlockBlob.UploadFromStreamAsync(uploadedImage);

                log.LogInformation($"[+] Stored failed image {blobName} into {ConfigurationSettings.FAILED_IMAGES_CONTAINERNAME} container as blob name: {convertedBlobName}");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to store a blob called {blobName} that failed conversion into {ConfigurationSettings.FAILED_IMAGES_CONTAINERNAME}. Exception ex {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the Job Table with a job status
        /// </summary>
        /// <param name="log"></param>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task UpdateJobTableWithStatus(ILogger log, string jobId, int status, string message, string conversionMode, string uploadedBlobUri)
        {
            JobTable jobTable = new JobTable(log, ConfigurationSettings.IMAGEJOBS_PARTITIONKEY);
            await jobTable.InsertOrReplaceJobEntity(jobId, status: status, message: message, conversionMode: conversionMode, uploadedBlobUri);
        }
    }
}
