namespace HW4AzureFunctionsSolution
{
    class ConfigurationSettings
    {
        public const string GRAYSCALEIMAGES_CONTAINERNAME = "converttogreyscale";
        
        public const string SEPIAIMAGES_CONTAINERNAME = "converttosepia";

        public const string CONVERTED_IMAGES_CONTAINERNAME = "convertedimages";

        public const string FAILED_IMAGES_CONTAINERNAME = "failedimages";

        public const string STORAGE_CONNECTIONSTRING_NAME = "AzureWebJobsStorage";

        public const string JOBS_TABLENAME = "hw4jobs";

        public const string IMAGEJOBS_PARTITIONKEY = "imageconversions";

        public const string JOBID_METADATA_NAME = "JobId";
    }
}
