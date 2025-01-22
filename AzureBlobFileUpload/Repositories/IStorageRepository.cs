using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobFileUpload.Repositories
{
    public interface IStorageRepository
    {
        Task UploadFile(string fileName, Stream fileStream, string contentType);
        Task UpdateFile(string fileName, Stream fileStream, string contentType);
        Task<(MemoryStream fileStream, string contentType)> GetFileAsync(string fileName);
        Task<List<string>> GetListOfBlobs();
        Task DeleteFile(string fileName);
    }
}
