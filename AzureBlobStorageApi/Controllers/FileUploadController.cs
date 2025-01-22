using AzureBlobStorageApi.DataTransferObjects;
using AzureBlobStorageApi.ExtensionMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureBlobStorageApi.Controllers
{
    /// <summary>
    /// Used to interact with storage container
    /// </summary>
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {

        private const string GET_BY_CONTAINER_AND_ID_ROUTE_NAME = "GetByContainerAndIdRouteName";

        private const string GET_FILE_BY_ID_AND_ROUTE_NAME = "GetFileByIdAndRouteName";
        
        /// <summary>
        /// Configuration getter and setter
        /// </summary>
        /// <value> App Configuration </value>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Retrieves the storage configuration string
        /// </summary>
        /// <value> Storage configuration string </value>
        public string StorageConnectionString
        {
            get
            {
                return Configuration.GetConnectionString("DefaultConnection");
            }
        }

        public FileUploadController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// First blob container
        /// </summary>
        private const string FIRST_CONTAINER = "myfirstcontainer";

        /// <summary>
        /// Public files container
        /// </summary>
        private const string PUBLIC_CONTAINER = "hw3container";

        /// <summary>
        /// Public files container
        /// </summary>
        private const string SECRET_CONTAINER = "secrethw3container";

        /// <summary>
        /// Provides information about a blob
        /// </summary>
        public class BlobName
        {
            /// <summary>
            /// Gets or sets the blob's  container.
            /// </summary>
            /// <value>The blob's container.</value>
            public string Container { get; set; }
            /// <summary>
            /// Gets or sets the blob name.
            /// </summary>
            /// <value>The blob's name.</value>
            public string Name { get; set; }
        }

        /// <summary>
        /// Uploads a file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="createPayload"></param>
        /// <returns>The name of the blob created</returns>
        [Route("{containername}/contentfiles/{filename}")]
        [ProducesDefaultResponseType]
        [HttpPut]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromRoute] CreatePayload createPayload)
        {
            if(!ModelState.IsValid)
            {
                return await RetrieveErrorsFromPayload();
            }
            
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            // Create Blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container
            CloudBlobContainer container = blobClient.GetContainerReference(PUBLIC_CONTAINER);

            // Create container if it doesn't exist
            await container.CreateIfNotExistsAsync();

            // Set permissions on blob to allow public access
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Retrieve the Blob referenced by caller
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(createPayload.FileName); 

            using (Stream uploadedFileStream = file.OpenReadStream())
            {
                blockBlob.Properties.ContentType = file.ContentType;
                await blockBlob.UploadFromStreamAsync(uploadedFileStream);
            }
            
            return CreatedAtRoute(GET_FILE_BY_ID_AND_ROUTE_NAME, new { id = createPayload.FileName }, null);
        }

        /// <summary>
        /// Get the file using the Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns> The file </returns>
        [Route("hw3container/{id}", Name = GET_FILE_BY_ID_AND_ROUTE_NAME)]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(FileResult))]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Produces("application/pdf","image/jpeg","image/png", "image/jpg", "text/html", "application/octet-stream")]
        [HttpGet]
        public Task<IActionResult> GetFileById(string id)
        {
            return RetrieveFile(containerName: PUBLIC_CONTAINER, Id: id);
        }

        /// <summary>
        /// Get A list of blob names in containers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFiles()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            List<BlobName> blobNames = new List<BlobName>();

            // Get the blobs in the public container
            await GetBlobnames(blobClient.GetContainerReference(FIRST_CONTAINER), blobNames);

            // Get the blobs in the private container
            await GetBlobnames(blobClient.GetContainerReference(SECRET_CONTAINER), blobNames);

            return (blobNames.Count > 0) ? new ObjectResult(blobNames.ToArray()) : new ObjectResult(null);
        }

        /// <summary>
        /// Retrieves a file using the container name and Id
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="Id"></param>
        /// <returns> The file </returns>
        private async Task<IActionResult> RetrieveFile(string containerName, string Id)
        {
            try
            {
                // Cantainer name must be lower case per azure rules
                containerName = containerName.ToLower();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

                // create blob client
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Retrieve container reference
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                // Verify that the requested container exists
                if(container == null || !(await container.ExistsAsync()))
                {
                    return BadRequest(new ErrorResponse()
                    {
                        errorNumber = 7,
                        parameterName = "containerName",
                        errorDescription = $"The Container {containerName} does not exist",
                        parameterValue = containerName
                    });
                }

                // Retrieve blob
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(Id);

                if(await blockBlob.ExistsAsync())
                {
                    MemoryStream memoryStream = new MemoryStream();
                    await blockBlob.DownloadToStreamAsync(memoryStream);

                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Retrieve the blob client
                    return new FileStreamResult(memoryStream, blockBlob.Properties.ContentType);
                }
                else
                {
                    return NotFound(new ErrorResponse()
                    {
                        errorNumber = 8,
                        parameterName = "Id",
                        errorDescription = $"The Container {containerName} does contain the blob named {Id}",
                        parameterValue = Id
                    });
                }

            }
            catch (StorageException sx)
            {
                WebException webException = sx.InnerException as WebException;

                if (webException != null)
                {
                    HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;

                    if (httpWebResponse != null)
                    {
                        return StatusCode((int)httpWebResponse.StatusCode, httpWebResponse.StatusDescription);
                    }
                    else
                    {
                        return BadRequest(webException.Message);
                    }

                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, sx.Message);
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Method to return custom error response based on payload validation rules
        /// </summary>
        /// <returns> IActionResult </returns>
        private async Task<IActionResult> RetrieveErrorsFromPayload()
        {
            List<ErrorResponse> errorResponses = new List<ErrorResponse>();

            // This allows us access to the raw input
            using StreamReader sr = new StreamReader(Request.Body);
            Request.Body.Seek(0, SeekOrigin.Begin);
            string inputJsonString = await sr.ReadToEndAsync();

            using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
            {
                foreach (string key in ModelState.Keys)
                {
                    if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                    {
                        foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                        {
                            string cleansedKey = key.CleanseModelStateKey();
                            string camelCaseKey = cleansedKey.ToCamelCase();

                            System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(camelCaseKey)}, errorMessage:{error.ErrorMessage}");

                            ErrorResponse errorResponse = new ErrorResponse();
                            (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                            errorResponse.parameterName = camelCaseKey;
                            errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(camelCaseKey).ToString();
                            errorResponses.Add(errorResponse);
                        }
                    }

                }
            }
            return BadRequest(errorResponses);
        }

        private async Task GetBlobnames(CloudBlobContainer container, List<BlobName> blobNames)
        {
            if(await container.ExistsAsync())
            {
                BlobRequestOptions options = new BlobRequestOptions();
                OperationContext context = new OperationContext();

                // Loop over items within the container and output the length and URI.
                BlobResultSegment result = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.Metadata, null, null, options, context);
                if (result?.Results != null)
                {
                    foreach (var blob in result.Results)
                    {
                        if (blob is CloudBlockBlob)
                        {
                            blobNames.Add(new BlobName() { Container = container.Name, Name = ((CloudBlockBlob)(blob)).Name });
                        }
                    }
                }
            }
        }

    }
}
