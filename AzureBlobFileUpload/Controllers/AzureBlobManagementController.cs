using Azure;
using AzureBlobFileManagement.DataTransferObjects;
using AzureBlobFileUpload.Repositories;
using AzureBlobFileUpload.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobFileUpload.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AzureBlobManagementController : ControllerBase
    {
        private const string GetFileByIdRoute = "GetFileByIdRoute";

        private readonly IStorageRepository _storageRepository;
        private IFileSettings _fileSettings;

        public AzureBlobManagementController(IStorageRepository storageRepository, IFileSettings fileSettings)
        {
            _storageRepository = storageRepository;
            _fileSettings = fileSettings;
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        [HttpPut]
        [Route("{containername}/contents/{filename}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromRoute] string containername, [FromRoute] string filename)
        {
            (bool isValid, List<ErrorResponse> errors) = isValidInput(file, containername, filename);

            if (!(isValid == true && errors.Count == 0))
            {
                return BadRequest(errors);
            }

            // Update the container name in the settings to the parameter value
            _fileSettings.FileContainerName = containername.ToLower();

            using Stream fileStream = file.OpenReadStream();

            try
            {
                await _storageRepository.UploadFile(filename, fileStream, file.ContentType);
            }
            catch (RequestFailedException)
            {
                return NoContent();
            }

            return CreatedAtRoute(GetFileByIdRoute, new { id = filename }, null);
        }

        /// <summary>
        /// Updates a file in the blob storage
        /// </summary>
        /// <param name="file"></param>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{containername}/contents/{filename}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateFile(IFormFile file, [FromRoute] string containername, [FromRoute] string filename)
        {
            (bool isValid, List<ErrorResponse> errors) = isValidInput(file, containername, filename);

            if (!(isValid == true && errors.Count == 0))
            {
                return BadRequest(errors);
            }

            try
            {
                (MemoryStream memoryStream, string contentType) = await _storageRepository.GetFileAsync(filename);
            }
            catch (RequestFailedException)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorNumber = 4,
                    ErrorDescription = ErrorResponse.GetErrorMessage(4),
                    ParameterName = "filename",
                    ParameterValue = filename
                });
            }

            // Update the container name in the settings to the parameter value
            _fileSettings.FileContainerName = containername.ToLower();

            using Stream fileStream = file.OpenReadStream();

            await _storageRepository.UpdateFile(filename, fileStream, file.ContentType);

            return NoContent();
        }

        /// <summary>
        /// Returns the File using the file name
        /// </summary>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <returns>The requested file</returns>
        [HttpGet]
        [Route("{containername}/contents/{filename}", Name = GetFileByIdRoute)]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFileByFilename(string containername, string filename)
        {
            // Update the container name in the settings to the parameter value
            _fileSettings.FileContainerName = containername.ToLower();

            try
            {
                (MemoryStream memoryStream, string contentType) = await _storageRepository.GetFileAsync(filename);
                return File(memoryStream, contentType);
            }
            catch (RequestFailedException)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorNumber = 4,
                    ErrorDescription = ErrorResponse.GetErrorMessage(4),
                    ParameterName = "filename",
                    ParameterValue = filename
                });
            }
        }

        /// <summary>
        /// Retrieves the names of the blobs in the specified container
        /// </summary>
        /// <returns>A list of blob names</returns>
        [HttpGet("/{containername}/contents")]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllFiles(string containername)
        {
            _fileSettings.FileContainerName = containername;

            try
            {
                return await _storageRepository.GetListOfBlobs();
            }
            catch (RequestFailedException)
            {
                return NotFound(new ErrorResponse()
                {
                    ErrorNumber = 4,
                    ErrorDescription = ErrorResponse.GetErrorMessage(4),
                    ParameterName = "containername",
                    ParameterValue = containername
                });
            }

            
        }

        /// <summary>
        /// Deletes a file using the provided container and file names
        /// </summary>
        /// <param name="containername"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpDelete("{containername}/contents/{filename}/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFile(string containername, string filename)
        {
            _fileSettings.FileContainerName = containername.ToLower();

            try
            {
                (MemoryStream memoryStream, string contentType) = await _storageRepository.GetFileAsync(filename);
            }
            catch (RequestFailedException)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorNumber = 4,
                    ErrorDescription = ErrorResponse.GetErrorMessage(4),
                    ParameterName = "filename",
                    ParameterValue = filename
                });
            }

            await _storageRepository.DeleteFile(filename);

            return NoContent();
        }

        private static (bool isValid, List<ErrorResponse> errors) isValidInput(IFormFile file, string filename, string containername)
        {
            List<ErrorResponse> fileValidationErrors = new List<ErrorResponse>();
            bool valid = true;


            if (filename == null)
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 6,
                    ErrorDescription = ErrorResponse.GetErrorMessage(6),
                    ParameterName = "fileName",
                    ParameterValue = "Null"
                });
                valid = false;
            }

            if (filename.Equals(""))
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 3,
                    ErrorDescription = ErrorResponse.GetErrorMessage(3),
                    ParameterName = "fileName",
                    ParameterValue = "Empty string"
                });
                valid = false;
            }

            if (filename.Length > 75)
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 2,
                    ErrorDescription = ErrorResponse.GetErrorMessage(2),
                    ParameterName = "fileName",
                    ParameterValue = filename
                });
                valid = false;
            }

            if (containername.Equals(""))
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 3,
                    ErrorDescription = ErrorResponse.GetErrorMessage(3),
                    ParameterName = "containername",
                    ParameterValue = "Empty string"
                });
                valid = false;
            }

            if (containername.Length < 3)
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 5,
                    ErrorDescription = ErrorResponse.GetErrorMessage(5),
                    ParameterName = "containername",
                    ParameterValue = containername
                });
                valid = false;
            }

            if (containername.Length > 63)
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 2,
                    ErrorDescription = ErrorResponse.GetErrorMessage(2),
                    ParameterName = "containername",
                    ParameterValue = containername
                });
                valid = false;
            }

            if (containername == null)
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 6,
                    ErrorDescription = ErrorResponse.GetErrorMessage(6),
                    ParameterName = "containername",
                    ParameterValue = "Null"
                });
                valid = false;
            }
            if (file == null)
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 6,
                    ErrorDescription = ErrorResponse.GetErrorMessage(6),
                    ParameterName = "file",
                    ParameterValue = "Null"
                });
                valid = false;
            }

            return (valid, fileValidationErrors);
        }
    }
}
