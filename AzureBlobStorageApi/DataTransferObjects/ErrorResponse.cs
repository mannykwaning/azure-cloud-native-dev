﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobStorageApi.DataTransferObjects
{
    /// <summary>
    /// Custom Error resonse definition
    /// </summary>
    public class ErrorResponse
    {

        public int errorNumber { get; set; }
        public string parameterName { get; set; }
        public string parameterValue { get; set; }
        public string errorDescription { get; set; }

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
                        return ("The entity could not be found", errorNumber);
                    }
                case 5:
                    {
                        return ("The parameter value is too small", errorNumber);
                    }
                case 6:
                    {
                        return ("The parameter cannot be null", errorNumber);
                    }
                default:
                    {
                        return ($"Raw Error: {encodedErrorDescription}", errorNumber);
                    }

            }
        }
    }
}
