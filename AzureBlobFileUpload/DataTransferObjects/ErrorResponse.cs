
namespace AzureBlobFileManagement.DataTransferObjects
{
    public class ErrorResponse
    {
        public int ErrorNumber { get; set; }

        public string ParameterName { get; set; }

        public string ParameterValue { get; set; }

        public string ErrorDescription { get; set; }

        public static string GetErrorMessage(int errorNumber)
        {
            switch (errorNumber)
            {
                case 1:
                    {
                        return ("The entity already exists");
                    }
                case 2:
                    {
                        return ("The parameter value is too large");
                    }
                case 3:
                    {
                        return ("The parameter is required");
                    }
                case 4:
                    {
                        return ("The entity could not be found");
                    }
                case 5:
                    {
                        return ("The parameter value is too small");
                    }
                case 6:
                    {
                        return ("The parameter cannot be null");
                    }
                default:
                    {
                        return ("Request parameter is invalid");
                    }

            }
        }

    }
}
