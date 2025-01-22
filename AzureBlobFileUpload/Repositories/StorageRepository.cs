using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using AzureBlobFileUpload.Settings;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobFileUpload.Repositories
{
    public class StorageRepository : IStorageRepository
    {
        private BlobContainerClient _blobContainerClient;
        private BlobServiceClient _blobServiceClient;
        private IStorageAccountSettings _storageAccountSettings;
        private IFileSettings _fileSettings;

        private bool IsInitialized { get; set; }

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

        public StorageRepository(IStorageAccountSettings storageAccountSettings,
                                IFileSettings fileSettings)
        {
            _storageAccountSettings = storageAccountSettings;
            _fileSettings = fileSettings;
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

            if(!_blobContainerClient.Exists())
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
                        blobNames.Add(blobItem.Name);
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
        private BlockBlobClient GetBlockBlobClient(string filename)
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
