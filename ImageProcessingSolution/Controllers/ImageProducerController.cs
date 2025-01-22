using Azure;
using Azure.Storage.Blobs.Specialized;
using ImageProducer.DataTransferObjects;
using ImageProducer.Entities;
using ImageProducer.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingSolution.Controllers
{
    [Route("api/v1")]
    [Produces("application/json")]
    [ApiController]
    public class ImageProducerController : ControllerBase
    {
        private const string GetFileByIdRoute = "GetFileByIdRoute";

        private readonly IStorageRepository _storageRepository;

        public ImageProducerController(IStorageRepository storageRepository)
        {
            _storageRepository = storageRepository;
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="imageConversionMode"></param>
        [HttpPut]
        [Route("uploadedimages/{imageConversionMode}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(IFormFile fileData, [FromRoute] string imageConversionMode)
        {
            (bool isValid, List<ErrorResponse> errors) = isValidInput(fileData, imageConversionMode);

            if (!(isValid == true && errors.Count == 0))
            {
                return BadRequest(errors);
            }

            Console.WriteLine($"conversion mode: {imageConversionMode}");

            string fileName = Guid.NewGuid().ToString();

            using Stream fileStream = fileData.OpenReadStream();

            try
            {
                await _storageRepository.UploadFile(fileName, fileStream, fileData.ContentType);
            }
            catch (RequestFailedException)
            {
                return BadRequest(new ErrorResponse()
                {
                    ErrorNumber = 2,
                    ParameterName = "fileName",
                    ParameterValue = $"{fileName}",
                    ErrorDescription = "The parameter provided is invalid.Valid parameter values are[1, 2, 3]"
                });
            }

            string queueMessage = $"{fileName} {imageConversionMode} uploadedimages";
            var encodeString = Encoding.UTF8.GetBytes(queueMessage);
            string convertedQmessage = Convert.ToBase64String(encodeString);

            await _storageRepository.EnQueueMessage(convertedQmessage);

            BlockBlobClient blockBlobClient = _storageRepository.GetBlockBlobClient(fileName);

            string blobUri = blockBlobClient.Uri.AbsoluteUri;

            await _storageRepository.InsertJob(imageConversionMode, fileName, 1, statusDescription: "Queued", blobUri, null);

            return StatusCode(201);
        }

        /// <summary>
        /// Returns the File using the file name
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>The requested file</returns>
        [HttpGet]
        [Route("uploadedimages/{filename}", Name = GetFileByIdRoute)]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFileByFilename(string filename)
        {
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
        /// Retrieves the names of the blobs in the uploadedimages container
        /// </summary>
        /// <returns>A list of blob names</returns>
        [HttpGet("/uploadedimages")]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllFiles()
        {
            try
            {
                List<string> blobs = await _storageRepository.GetListOfBlobs();
                return new OkObjectResult(blobs);
            }
            catch (RequestFailedException)
            {
                return NotFound(new ErrorResponse()
                {
                    ErrorNumber = 4,
                    ErrorDescription = ErrorResponse.GetErrorMessage(4),
                });
            }
        }

        /// <summary>
        /// Retrieves all the jobs in the jobs table
        /// </summary>
        /// <returns>A list of Jobs</returns>
        [HttpGet("/jobs")]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllJobs()
        {
            try
            {
                List<ImageConversionJobs> jobs = await _storageRepository.RetrieveAllJobs();
                return new OkObjectResult(jobs);
            }
            catch (Exception)
            {
                return NoContent();
            }
        }

        /// <summary>
        /// Retrieves A single job from job table via JobId/ Blob Id
        /// </summary>
        /// <param name="imageConversionMode"></param>
        /// <param name="id"></param>
        /// <returns>A Job</returns>
        [HttpGet("/imageconversionmodes/{imageConversionMode}/jobs/{id}")]
        [ProducesResponseType(typeof(Stream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetJob([FromRoute] string imageConversionMode, [FromRoute] string id)
        {

            (bool isValid, List<ErrorResponse> errors) = isValidJobParams(id, imageConversionMode);

            if (!(isValid == true && errors.Count == 0))
            {
                return BadRequest(errors);
            }

            try
            {
                ImageConversionJobs job = await _storageRepository.RetrieveJob(imageConversionMode, id);

                if (job.RowKey == null)
                {
                    return NotFound(new ErrorResponse()
                    {
                        ErrorNumber = 4,
                        ParameterName = "id",
                        ParameterValue = $"{id}",
                        ErrorDescription = "The entity could not be found"
                    });
                }

                return new OkObjectResult(job);
            }
            catch (Exception)
            {
                return NoContent();
            }
        }

        /// <summary>
        /// Validates request object and parameters
        /// </summary>
        /// <param name="file"></param>
        /// <param name="conversionMode"></param>
        /// <returns></returns>
        private static (bool isValid, List<ErrorResponse> errors) isValidInput(IFormFile file, string conversionMode)
        {
            List<ErrorResponse> fileValidationErrors = new List<ErrorResponse>();
            List<String> validConversionMode = new List<string> { "1", "2", "3" };
            bool valid = true;

            if (conversionMode.Equals(null))
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 5,
                    ErrorDescription = "The parameter provided is invalid. Valid parameter values are [1, 2, 3]",
                    ParameterName = "fileName",
                    ParameterValue = "Null"
                });
                valid = false;
            }

            if (!validConversionMode.Contains(conversionMode))
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 2,
                    ErrorDescription = "The parameter provided is invalid. Valid parameter values are [1, 2, 3]",
                    ParameterName = "conversionMode",
                    ParameterValue = $"{conversionMode}"
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

        /// <summary>
        /// Validated request parameters for get job by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="conversionMode"></param>
        /// <returns> true/false </returns>
        private static (bool isValid, List<ErrorResponse> errors) isValidJobParams(string id, string conversionMode)
        {
            List<ErrorResponse> fileValidationErrors = new List<ErrorResponse>();
            List<String> validConversionMode = new List<string> { "1", "2", "3" };
            bool valid = true;

            if (conversionMode.Equals(null))
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 5,
                    ErrorDescription = "The parameter provided is invalid. Valid parameter values are [1, 2, 3]",
                    ParameterName = "fileName",
                    ParameterValue = "Null"
                });
                valid = false;
            }

            if (!validConversionMode.Contains(conversionMode))
            {
                fileValidationErrors.Add(new ErrorResponse()
                {
                    ErrorNumber = 2,
                    ErrorDescription = "The parameter provided is invalid. Valid parameter values are [1, 2, 3]",
                    ParameterName = "conversionMode",
                    ParameterValue = $"{conversionMode}"
                });
                valid = false;
            }

            if (id == null)
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
