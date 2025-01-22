using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Queues;
using ImageProducer.Entities;
using ImageProducer.Settings;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageProducer.Repositories
{
    public class StorageRepository : IStorageRepository
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const string IMAGES_TO_PROCESS_QUEUE = "imagestoprocessqueue";
        private const string IMAGE_CONVERSION_JOBS = "imageconversionjobs";

        /// <summary>
        /// Blob Clients
        /// </summary>
        private BlobContainerClient _blobContainerClient;
        private BlobServiceClient _blobServiceClient;

        /// <summary>
        /// Queue Clients
        /// </summary>
        private QueueServiceClient _queueServiceClient;
        private QueueClient _queueClient;

        /// <summary>
        /// Table Clients
        /// </summary>
        private CloudTableClient _cloudTableClient;
        private CloudTable _cloudTable;

        /// <summary>
        /// Storage Account settings
        /// </summary>
        private IStorageAccountSettings _storageAccountSettings;

        /// <summary>
        /// File Settings
        /// </summary>
        private IFileSettings _fileSettings;

        private bool IsInitialized { get; set; }
        private bool QueueInitialized { get; set; }
        private bool TableInitialized { get; set; }

        public StorageRepository(IStorageAccountSettings storageAccountSettings,
                                IFileSettings fileSettings)
        {
            _storageAccountSettings = storageAccountSettings;
            _fileSettings = fileSettings;
        }

        /// <summary>
        /// Initialize this instance - not thread safe
        /// Gets the settings name from the properties,
        /// Determines access type from string substring per requirements
        /// </summary>
        private void Initialize()
        {
            if (!IsInitialized)
            {
                _blobServiceClient = new BlobServiceClient(_storageAccountSettings.StorageAccountConnectionString);

                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_fileSettings.FileContainerName);

                // Parse container name string to determine access type
                if (_fileSettings.FileContainerName.Contains("public"))
                {
                    _blobContainerClient.CreateIfNotExists(publicAccessType: PublicAccessType.BlobContainer);
                }
                else
                {
                    _blobContainerClient.CreateIfNotExists(publicAccessType: PublicAccessType.None);
                }

                IsInitialized = true;
            }
        }

        /// <summary>
        /// Initialize Queue Client
        /// </summary>
        private void InitQueueClient()
        {
            if (!QueueInitialized)
            {
                _queueServiceClient = new QueueServiceClient(_storageAccountSettings.StorageAccountConnectionString);
                _queueClient = _queueServiceClient.GetQueueClient(IMAGES_TO_PROCESS_QUEUE);

                _queueClient.CreateIfNotExists();
            }
            QueueInitialized = true;
        }

        /// <summary>
        /// Returns the initialized queue client
        /// </summary>
        /// <returns></returns>
        private QueueClient GetQueueClient()
        {
            if (!QueueInitialized)
            {
                InitQueueClient();
            }
            return _queueClient;
        }

        /// <summary>
        /// Initialize Table Client
        /// </summary>
        private void InitTableClient()
        {
            if (!TableInitialized)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageAccountSettings.StorageAccountConnectionString);

                _cloudTableClient = storageAccount.CreateCloudTableClient();
                _cloudTable = _cloudTableClient.GetTableReference(IMAGE_CONVERSION_JOBS);

                _cloudTable.CreateIfNotExists();
            }
            TableInitialized = true;
        }

        /// <summary>
        /// Returns the initialized table client
        /// </summary>
        /// <returns></returns>
        private CloudTable GetCloudTable()
        {
            if (!TableInitialized)
            {
                InitTableClient();
            }
            return _cloudTable;
        }

        /// <summary>
        /// The blob container client
        /// </summary>
        private BlobContainerClient GetBlobContainerClient()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            return _blobContainerClient;
        }

        /// <summary>
        /// Inserts job record into job table
        /// </summary>
        /// <param name="conversionMode"></param>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        /// <param name="statusDescription"></param>
        /// <param name="imageSource"></param>
        /// <param name="imageResult"></param>
        /// <returns></returns>
        public async Task InsertJob(string conversionMode, string jobId, int status, string statusDescription, string imageSource, string imageResult)
        {
            ImageConversionJobs jobEntity = new ImageConversionJobs()
            {
                PartitionKey = conversionMode,
                RowKey = jobId,
                Status = status,
                StatusDescription = statusDescription,
                ImageSource = imageSource,
                ImageResult = imageResult
            };

            CloudTable cloudTable = GetCloudTable();
            TableOperation insertReplace = TableOperation.InsertOrReplace(jobEntity);

            await cloudTable.ExecuteAsync(insertReplace);
        }

        /// <summary>
        /// Retrieves a single job via jobId
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns> Job Row. </returns>
        public async Task<ImageConversionJobs> RetrieveJob(string partitionKey, string rowKey)
        {
            CloudTable cloudTable = GetCloudTable();
            TableOperation retrieve = TableOperation.Retrieve<ImageConversionJobs>(partitionKey, rowKey);
            ImageConversionJobs job = new ImageConversionJobs();

            TableResult query = await cloudTable.ExecuteAsync(retrieve);
            if(query.Result != null)
            {
                job.RowKey = ((ImageConversionJobs)query.Result).RowKey;
                job.PartitionKey = ((ImageConversionJobs)query.Result).PartitionKey;
                job.Status = ((ImageConversionJobs)query.Result).Status;
                job.StatusDescription = ((ImageConversionJobs)query.Result).StatusDescription;
                job.ImageSource = ((ImageConversionJobs)query.Result).ImageSource;
                job.ImageResult = ((ImageConversionJobs)query.Result).ImageResult;
            }

            return (ImageConversionJobs) job;
        }

        /// <summary>
        /// Retrieve all jobs in jobs table
        /// </summary>
        /// <returns> List of jobs. </returns>
        public async Task<List<ImageConversionJobs>> RetrieveAllJobs()
        {
            CloudTable cloudTable = GetCloudTable();

            TableQuery<ImageConversionJobs> query = new TableQuery<ImageConversionJobs>();
            List<ImageConversionJobs> imageConversionJobs = new List<ImageConversionJobs>();

            TableContinuationToken ContinuationToken = null;
            do
            {
                var page = await cloudTable.ExecuteQuerySegmentedAsync(query, ContinuationToken);
                ContinuationToken = page.ContinuationToken;
                imageConversionJobs.AddRange(page.Results);
            } while (ContinuationToken != null);

            return imageConversionJobs;
        }

        /// <summary>
        /// Inserts Message intto queue
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task EnQueueMessage(string message)
        {
            QueueClient queueClient = GetQueueClient();
            await queueClient.SendMessageAsync(message);
        }

        /// <summary>
        /// Uploads a file to blob storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task UploadFile(string fileName, Stream fileStream, string contentType)
        {
            BlobClient blobClient = GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream);
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders() { ContentType = contentType });
        }

        /// <summary>
        /// Updates an existing file in the blob storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task UpdateFile(string fileName, Stream fileStream, string contentType)
        {
            BlockBlobClient blockBlobClient = GetBlockBlobClient(fileName);
            await blockBlobClient.UploadAsync(fileStream);
            await blockBlobClient.SetHttpHeadersAsync(new BlobHttpHeaders() { ContentType = contentType });
        }

        /// <summary>
        /// Deletes a blob from storage
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task DeleteFile(string fileName)
        {
            var blob = GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Retrieve the names of all the blobs in the container
        /// </summary>
        /// <returns>List of blob names</returns>
        public async Task<List<string>> GetListOfBlobs()
        {
            _blobServiceClient = new BlobServiceClient(_storageAccountSettings.StorageAccountConnectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_fileSettings.FileContainerName);

            if (!_blobContainerClient.Exists())
            {
                throw new RequestFailedException("The requested container was not found");
            }
            else
            {
                BlobContainerClient blobContainerClient = GetBlobContainerClient();
                var blobs = blobContainerClient.GetBlobsAsync();

                List<string> blobNames = new List<string>();

                await foreach (var blobPage in blobs.AsPages())
                {
                    foreach (var blobItem in blobPage.Values)
                    {
                        blobNames.Add("id: " + blobItem.Name);
                    }

                }
                return blobNames;
            }
        }

        /// <summary>
        /// Get's the associated block blob client from the filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public BlockBlobClient GetBlockBlobClient(string filename)
        {
            BlobContainerClient blobContainerClient = GetBlobContainerClient();
            return blobContainerClient.GetBlockBlobClient(filename);
        }

        /// <summary>
        /// Get's the associated blob client from the filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private BlobClient GetBlobClient(string fileName)
        {
            BlobContainerClient blobContainerClient = GetBlobContainerClient();
            return blobContainerClient.GetBlobClient(fileName);
        }

        /// <summary>
        /// Gets the file from the blob storage
        /// </summary>
        /// <param name="fileName">Id of the blob bieng downloaded</param>
        /// <returns>Memory stream containing the blob - must be disposed afterwards</returns>
        public async Task<(MemoryStream fileStream, string contentType)> GetFileAsync(string fileName)
        {
            BlobClient blobClient = GetBlobClient(fileName);
            using BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();

            // Memory stream must be disposed afterwards
            MemoryStream memoryStream = new MemoryStream();
            await blobDownloadInfo.Content.CopyToAsync(memoryStream);

            // Reset the stream to the beginning so readers don't have to
            memoryStream.Position = 0;
            return (memoryStream, blobDownloadInfo.ContentType);
        }

    }
}
