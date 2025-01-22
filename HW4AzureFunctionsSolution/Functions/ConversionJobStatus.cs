using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HW4AzureFunctionsSolution.Functions
{
    public static class ConversionJobStatus
    {
        /// <summary>
        /// HW: 4 #5
        /// HTTP Triggered Azure function to retrieve all Blobs in container
        /// with partition Key
        /// </summary>
        /// <param name="req"></param>
        /// <param name="key"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("ConversionJobStatus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/jobs/{key}")] HttpRequest req, string key,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            log.LogInformation($"Recieved the query: {name} with Key: {key}");

            if (key == null)
            {
                return new NotFoundObjectResult(new ErrorResponse()
                {
                    ErrorNumber = 4,
                    ParameterName = "JobId",
                    ParameterValue = key,
                    ErrorDescription = "The parameter cannot be null."
                });
            }
            if (key == "")
            {
                return new NotFoundObjectResult(new ErrorResponse()
                {
                    ErrorNumber = 2,
                    ParameterName = "JobId",
                    ParameterValue = key,
                    ErrorDescription = "The parameter is required."
                });
            }

            JobTable jobTable = new JobTable(log, key);

            List<JobEntity> jobs = await jobTable.RetrieveAllJobs();

            return new OkObjectResult(jobs);
        }
    }
}

