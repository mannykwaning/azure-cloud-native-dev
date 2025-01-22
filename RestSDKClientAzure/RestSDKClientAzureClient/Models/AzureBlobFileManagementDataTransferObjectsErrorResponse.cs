﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace RestSDKClientAzure.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;

    public partial class AzureBlobFileManagementDataTransferObjectsErrorResponse
    {
        /// <summary>
        /// Initializes a new instance of the
        /// AzureBlobFileManagementDataTransferObjectsErrorResponse class.
        /// </summary>
        public AzureBlobFileManagementDataTransferObjectsErrorResponse() { }

        /// <summary>
        /// Initializes a new instance of the
        /// AzureBlobFileManagementDataTransferObjectsErrorResponse class.
        /// </summary>
        public AzureBlobFileManagementDataTransferObjectsErrorResponse(int? errorNumber = default(int?), string parameterName = default(string), string parameterValue = default(string), string errorDescription = default(string))
        {
            ErrorNumber = errorNumber;
            ParameterName = parameterName;
            ParameterValue = parameterValue;
            ErrorDescription = errorDescription;
        }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "errorNumber")]
        public int? ErrorNumber { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "parameterName")]
        public string ParameterName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "parameterValue")]
        public string ParameterValue { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorDescription { get; set; }

    }
}
