﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace RestSDKClientAzure
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest;
    using Models;

    /// <summary>
    /// Extension methods for RestSDKClientAzureClient.
    /// </summary>
    public static partial class RestSDKClientAzureClientExtensions
    {
            /// <summary>
            /// Uploads a file
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            /// <param name='file'>
            /// </param>
            public static IList<AzureBlobFileManagementDataTransferObjectsErrorResponse> UploadFile(this IRestSDKClientAzureClient operations, string containername, string filename, System.IO.Stream file = default(System.IO.Stream))
            {
                return Task.Factory.StartNew(s => ((IRestSDKClientAzureClient)s).UploadFileAsync(containername, filename, file), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Uploads a file
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            /// <param name='file'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IList<AzureBlobFileManagementDataTransferObjectsErrorResponse>> UploadFileAsync(this IRestSDKClientAzureClient operations, string containername, string filename, System.IO.Stream file = default(System.IO.Stream), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.UploadFileWithHttpMessagesAsync(containername, filename, file, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Updates a file in the blob storage
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            /// <param name='file'>
            /// </param>
            public static object UpdateFile(this IRestSDKClientAzureClient operations, string containername, string filename, System.IO.Stream file = default(System.IO.Stream))
            {
                return Task.Factory.StartNew(s => ((IRestSDKClientAzureClient)s).UpdateFileAsync(containername, filename, file), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Updates a file in the blob storage
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            /// <param name='file'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> UpdateFileAsync(this IRestSDKClientAzureClient operations, string containername, string filename, System.IO.Stream file = default(System.IO.Stream), CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.UpdateFileWithHttpMessagesAsync(containername, filename, file, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Returns the File using the file name
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            public static object GetFileByFilename(this IRestSDKClientAzureClient operations, string containername, string filename)
            {
                return Task.Factory.StartNew(s => ((IRestSDKClientAzureClient)s).GetFileByFilenameAsync(containername, filename), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Returns the File using the file name
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> GetFileByFilenameAsync(this IRestSDKClientAzureClient operations, string containername, string filename, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetFileByFilenameWithHttpMessagesAsync(containername, filename, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Deletes a file using the provided container and file names
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            public static AzureBlobFileManagementDataTransferObjectsErrorResponse DeleteFile(this IRestSDKClientAzureClient operations, string containername, string filename)
            {
                return Task.Factory.StartNew(s => ((IRestSDKClientAzureClient)s).DeleteFileAsync(containername, filename), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Deletes a file using the provided container and file names
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='filename'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<AzureBlobFileManagementDataTransferObjectsErrorResponse> DeleteFileAsync(this IRestSDKClientAzureClient operations, string containername, string filename, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.DeleteFileWithHttpMessagesAsync(containername, filename, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

            /// <summary>
            /// Retrieves the names of the blobs in the specified container
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            public static object GetAllFiles(this IRestSDKClientAzureClient operations, string containername)
            {
                return Task.Factory.StartNew(s => ((IRestSDKClientAzureClient)s).GetAllFilesAsync(containername), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Retrieves the names of the blobs in the specified container
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='containername'>
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<object> GetAllFilesAsync(this IRestSDKClientAzureClient operations, string containername, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetAllFilesWithHttpMessagesAsync(containername, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}
