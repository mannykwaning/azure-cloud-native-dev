using ImageProcessor;
using ImageProcessor.Imaging.Filters.Photo;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

namespace ImageConsumer
{
    public class Functions
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const string IMAGES_TO_PROCESS_QUEUE = "imagestoprocessqueue";
        private const string UPLOADED_IMAGES_CONTAINER = "uploadedimages";
        private const string CONVERTED_IMAGES_CONTAINER = "convertedimages";
        private const string IMAGE_CONVERSION_JOBS = "imageconversionjobs";

        /// <summary>
        /// This function will get triggered/executed via logic app
        /// </summary>
        /// <param name="log"></param>
        [NoAutomaticTrigger]
        public static void ProcessQueueMessage(TextWriter log)
        {
            CloudQueue procQueue = GetCloudQueue();
            var message = procQueue.GetMessage();
            string messageText;

            do
            {
                try
                {
                    messageText = message.AsString;
                }
                catch (Exception e)
                {
                    log.WriteLine($"An Exception Occured with message: {e.Message}");
                    Console.WriteLine($"An Exception Occured with message: {e.Message}");
                    return;
                }

                log.WriteLine($"Message: {messageText}");

                string[] words = messageText.Split(' ');
                if (!words[2].Equals("uploadedimages") || !(words.Length == 3))
                {
                    log.WriteLine($"An Invalid message was recieved: {messageText}");
                    Console.WriteLine($"An Invalid message was recieved: {messageText}");
                    break;
                }

                QMessage qMessage = new QMessage()
                {
                    Id = words[0],
                    ConversionMode = words[1],
                    ContainerName = words[2]
                };

                UpdateJobTable(qMessage, log, status: 2, description: "Job is running", blobUri: null);

                CloudBlobContainer cloudBlobContainer = GetBlobClient(UPLOADED_IMAGES_CONTAINER);

                CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(qMessage.Id);

                //Convert uploaded image and deposit in converted images container
                using (Stream blobStream = blockBlob.OpenRead())
                {
                    //Retrieve converted images container
                    CloudBlobContainer convertedImagesContainer = GetBlobClient(CONVERTED_IMAGES_CONTAINER);

                    string convertedBlobUri = ConvertImageAndUpload(qMessage.ConversionMode, blobStream, convertedImagesContainer, qMessage.Id, log);

                    if (!convertedBlobUri.Equals(""))
                    {
                        UpdateJobTable(qMessage, log, status: 3, description: "Job completed with success", convertedBlobUri);
                    }
                    else
                    {
                        UpdateJobTable(qMessage, log, status: 4, description: "Job conversion failed", null);
                    }
                }

                procQueue.DeleteMessage(message);

                message = procQueue.GetMessage();
            } while (message != null);
        }

        private static string ConvertImageAndUpload(string conversionMode,
                                                                     Stream uploadedImage,
                                                                     CloudBlobContainer convertedImagesContainer,
                                                                     string blobId,
                                                                     TextWriter log)
        {
            string convertedBlobUrl = "";
            try
            {
                using (MemoryStream convertedMemoryStream = new MemoryStream())
                using (var factory = new ImageFactory(preserveExifData: true))
                {
                    switch (conversionMode)
                    {
                        case "1":
                            factory.Load(uploadedImage)
                                .Filter(MatrixFilters.GreyScale)
                                .Save(convertedMemoryStream);
                            break;
                        case "2":
                            factory.Load(uploadedImage)
                                .Filter(MatrixFilters.Sepia)
                                .Save(convertedMemoryStream);
                            break;
                        case "3":
                            factory.Load(uploadedImage)
                                .Filter(MatrixFilters.Comic)
                                .Save(convertedMemoryStream);
                            break;
                        default:
                            log.Write($"{conversionMode} is invalid");
                            break;
                    }

                    CloudBlockBlob convertedBlockBlob = convertedImagesContainer.GetBlockBlobReference(blobId);
                    convertedBlockBlob.Metadata.Add("BlobId", blobId);
                    convertedBlockBlob.Properties.ContentType = MediaTypeNames.Image.Jpeg;

                    convertedBlockBlob.UploadFromStream(convertedMemoryStream);
                    convertedBlobUrl = convertedBlockBlob.Uri.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Write($"Exception Occured: {ex.Message}");
                Console.WriteLine($"Exception Occured: {ex.Message}");
            }
            return convertedBlobUrl;
        }

        /// <summary>
        /// Creates and returns cloud queue
        /// </summary>
        /// <returns>CloudQueue</returns>
        private static CloudQueue GetCloudQueue()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference(IMAGES_TO_PROCESS_QUEUE);

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();

            return queue;
        }

        /// <summary>
        /// Creates and returns cloud table
        /// </summary>
        /// <returns>CloudTable</returns>
        private static CloudTable GetCloudTable()
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(IMAGE_CONVERSION_JOBS);

            table.CreateIfNotExistsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            return table;
        }


        private static CloudBlobContainer GetBlobClient(string containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer convertedImagesContainer = blobClient.GetContainerReference(containerName);

            convertedImagesContainer.CreateIfNotExists();

            return convertedImagesContainer;
        }

        /// <summary>
        /// Updates Job Table
        /// </summary>
        /// <param name="status"></param>
        /// <param name="description"></param>
        private static void UpdateJobTable(QMessage qMessage,
                                                 TextWriter log,
                                                 int status,
                                                 string description,
                                                 string blobUri)
        {
            ImageConversionJobs jobToReplace = RetrieveJob(qMessage.ConversionMode, qMessage.Id);
            if (jobToReplace != null)
            {
                jobToReplace.Status = status;
                jobToReplace.StatusDescription = description;
                jobToReplace.ImageResult = blobUri;

                TableOperation insertOrUpdate = TableOperation.InsertOrMerge(jobToReplace);
                CloudTable cloudTable = GetCloudTable();

                cloudTable.Execute(insertOrUpdate);
            }
            else
            {
                log.Write($"Job with PartitionKey: {qMessage.ConversionMode} && RowKey: {qMessage.Id}, doesn't exist");
            }
        }

        /// <summary>
        /// Retrieves Job via JobId/ RowKey && partitionKey/ imageConversionMode
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns>Job</returns>
        private static ImageConversionJobs RetrieveJob(string partitionKey, string rowKey)
        {
            CloudTable cloudTable = GetCloudTable();
            TableOperation retrieve = TableOperation.Retrieve<ImageConversionJobs>(partitionKey, rowKey);
            ImageConversionJobs job = new ImageConversionJobs();

            TableResult query = cloudTable.Execute(retrieve);
            if (query.Result != null)
            {
                job.RowKey = ((ImageConversionJobs)query.Result).RowKey;
                job.PartitionKey = ((ImageConversionJobs)query.Result).PartitionKey;
                job.Status = ((ImageConversionJobs)query.Result).Status;
                job.StatusDescription = ((ImageConversionJobs)query.Result).StatusDescription;
                job.ImageSource = ((ImageConversionJobs)query.Result).ImageSource;
                job.ImageResult = ((ImageConversionJobs)query.Result).ImageResult;
            }

            return (ImageConversionJobs)job;
        }

        /// <summary>
        /// Qmessage POCO
        /// </summary>
        private class QMessage
        {
            public string Id { get; set; }
            public string ConversionMode { get; set; }
            public string ContainerName { get; set; }

        }

        /// <summary>
        /// Job Table Entity
        /// </summary>
        private class ImageConversionJobs : TableEntity
        {
            public int Status { get; set; }
            public string StatusDescription { get; set; }
            public string ImageSource { get; set; }
            public string ImageResult { get; set; }
        }
    }
}
