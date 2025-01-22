using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HW4AzureFunctionsSolution.Functions
{
    public static class ConversionJobStatusById
    {
        /// <summary>
        /// HW:4 #6 
        /// HTTP Triggered Azure function to retrieve a Blob By UserID
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ConversionJobStatusById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/jobs/{id}")] HttpRequest req, string id, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            log.LogInformation($"Recieved the query: {name} with id: {id}");

            if (id == null)
            {
                return new NotFoundObjectResult(new ErrorResponse()
                {
                    ErrorNumber = 4,
                    ParameterName = "JobId",
                    ParameterValue = id,
                    ErrorDescription = "The parameter cannot be null."
                });
            }
            if (id == "")
            {
                return new NotFoundObjectResult(new ErrorResponse()
                {
                    ErrorNumber = 2,
                    ParameterName = "JobId",
                    ParameterValue = id,
                    ErrorDescription = "The parameter is required."
                });
            }

            JobTable jobTable = new JobTable(log, ConfigurationSettings.IMAGEJOBS_PARTITIONKEY);

            JobEntity jobEntity = await jobTable.RetrieveJobEntity(id);

            if(jobEntity == null)
            {
                return new NotFoundObjectResult(new ErrorResponse()
                {
                    ErrorNumber = 3,
                    ParameterName = "JobId",
                    ParameterValue = id,
                    ErrorDescription = "The entity could not be found."
                });
            }

            return new OkObjectResult(new JobEntity()
            {
                JobId = id,
                ImageConversionMode = jobEntity.ImageConversionMode,
                Status = jobEntity.Status,
                StatusDescription = jobEntity.StatusDescription,
                ImageSource = jobEntity.ImageSource,
                ImageResult = jobEntity.ImageResult
            });

        }
    }
}
