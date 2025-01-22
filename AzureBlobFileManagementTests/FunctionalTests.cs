using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSDKClientAzure;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AzureBlobFileManagementTests
{
    [TestClass]
    public class FunctionalTests
    {
        ServiceClientCredentials _serviceClientCredentials;
        private RestSDKClientAzureClient _client;

        [TestInitialize]
        public void Initialize()
        {
            _serviceClientCredentials = new TokenCredentials("FakeTokenValue");

            _client = new RestSDKClientAzureClient(
                new Uri("https://localhost:5001/"), _serviceClientCredentials);
        }

        /// <summary>
        /// Note: the file get's uploaded, change the filename and container name to test.
        /// For some reason rest client doesn't return the right httpStatusCode
        /// </summary>
        /// <returns></returns>
        [TestMethod("(●'◡'●) Test file was sucessfully uploaded")]
        public async Task TestFileUpload()
        {
            // Arrange
            string fileName = "MJ.jpg";
            string containerName = "publicfunctionaltests";

            using System.IO.FileStream fileStream = System.IO.File.OpenRead(fileName);

            // Act
            HttpOperationResponse response = await _client.UploadFileWithHttpMessagesAsync(containerName, fileName, fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg);

            // Assert
            Assert.IsTrue(response.Response.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.NoContent, response.Response.StatusCode);
        }

        [TestMethod("(ง •_•)ง Test that if a parameter is null A validation exception is thrown")]
        [ExpectedException(typeof(ValidationException))]
        public async Task TestContainerNameRequired()
        {
            // Arrange
            string fileName = "MJ.jpg";
            string containerName = null;

            using System.IO.FileStream fileStream = System.IO.File.OpenRead(fileName);

            // Act
            HttpOperationResponse response = await _client.UploadFileWithHttpMessagesAsync(containerName, fileName, fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg);

            // Assert
            // ValidationException is thrown
        }
        [TestMethod("(●'◡'●) Test that the file was retrieved")]
        public async Task TestGetByFileNameAndContainerName()
        {
            // Arrange
            string fileName = "MJ.jpg";
            string containerName = "publicfunctionaltests";

            // Act
            HttpOperationResponse response = await _client.GetFileByFilenameWithHttpMessagesAsync(containerName, fileName);
            string contentType = response.Response.Content.Headers.ContentType.MediaType;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.Response.StatusCode);
            Assert.AreEqual("image/jpeg", contentType);
        }

        [TestMethod("ψ(._. )> Test that the file was not found")]
        public async Task TestFileNotFound()
        {
            // Arrange
            string fileName = "buggy.jpg";
            string containerName = "publicfunctionaltests";

            try
            {
                // Act
                var response = await _client.GetFileByFilenameWithHttpMessagesAsync(containerName, fileName);
            }
            catch (HttpOperationException ex)
            {
                // Assert
                Assert.AreEqual(ex.Response.StatusCode, HttpStatusCode.BadRequest);
            }            
        }
    }
}
