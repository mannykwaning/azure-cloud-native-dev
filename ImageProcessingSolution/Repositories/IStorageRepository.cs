using Azure.Storage.Blobs.Specialized;
using ImageProducer.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageProducer.Repositories
{
    public interface IStorageRepository
    {
        Task UploadFile(string fileName, Stream fileStream, string contentType);
        Task UpdateFile(string fileName, Stream fileStream, string contentType);
        Task<(MemoryStream fileStream, string contentType)> GetFileAsync(string fileName);
        BlockBlobClient GetBlockBlobClient(string filename);
        Task<List<string>> GetListOfBlobs();
        Task DeleteFile(string fileName);
        Task EnQueueMessage(string message);
        Task<List<ImageConversionJobs>> RetrieveAllJobs();
        Task<ImageConversionJobs> RetrieveJob(string partitionKey, string rowKey);
        Task InsertJob(string conversionMode, string jobId, int status, string statusDescription, string imageSource, string imageResult);
    }
}