using System.ComponentModel.DataAnnotations;

namespace TodoTasksApp.DataTransferObjects
{
    /// <summary>
    /// Custom Error Response Definition
    /// </summary>
    public class ErrorResponse
    {
        public int ErrorNumber { get; set; }

        public string ParameterName { get; set; }

        public string ParameterValue { get; set; }

        public string ErrorDescription { get; set; }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error number
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error number</returns>
        public static int GetErrorNumberFromDescription(string encodedErrorDescription)
        {
            if (int.TryParse(encodedErrorDescription, out int errorNumber))
            {
                return errorNumber;
            }
            return 0;
        }

        /// <summary>
        /// Converts an error number inside an encoded error description, to the standard error response
        /// </summary>
        /// <param name="encodedErrorDescription">The error description</param>
        /// <returns>The decoded error message and number</returns>
        public static (string decodedErrorMessage, int decodedErrorNumber) GetErrorMessage(string encodedErrorDescription)
        {

            int errorNumber = GetErrorNumberFromDescription(encodedErrorDescription);

            switch (errorNumber)
            {
                case 1:
                    {
                        return ("The entity already exists", errorNumber);
                    }
                case 2:
                    {
                        return ("The parameter value is too large", errorNumber);
                    }
                case 3:
                    {
                        return ("The parameter is required", errorNumber);
                    }
                case 4:
                    {
                        return ("The maximum number of entities have been created.No further entities can be created at this time.", errorNumber);
                    }
                case 5:
                    {
                        return ("The entity could not be found", errorNumber);
                    }
                case 6:
                    {
                        return ("The parameter value is too small", errorNumber);
                    }
                case 7:
                    {
                        return ("The parameter value is not valid", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }

            }
        }

    }
}
