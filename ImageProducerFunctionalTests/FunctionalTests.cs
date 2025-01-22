using ImageProcessingRestSdk;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageProducerFunctionalTests
{
    [TestClass]
    public class FunctionalTests
    {
        private ServiceClientCredentials _serviceClientCredentials;
        private RestSdkClientLibrary _client;
        private HttpClient httpClient;

        [TestInitialize]
        public void Initialize()
        {
            _serviceClientCredentials = new TokenCredentials("FakeTokenValue");

            _client = new RestSdkClientLibrary("https://localhost:5001/", httpClient);
        }

        [TestMethod("(●'◡'●) Test file was sucessfully uploaded")]
        public async Task TestFileUpload()
        {
            // Arrange
            string fileName = "eminem.jpg";

            System.IO.FileStream fileStream = System.IO.File.OpenRead(fileName);

            FileParameter file = new FileParameter(fileStream);

            // Act
            int response = await _client.UploadFileAsync("1", file);

            // Assert
            Assert.IsTrue(response.Equals(201));
        }

        [TestMethod("ψ(._. )> Test that the file was not found")]
        public async Task TestFileNotFound()
        {
            // Arrange
            string fileName = "buggy.jpg";

            try
            {
                // Act
                var response = await _client.GetFileByFilenameAsync(fileName);
            }
            catch (HttpOperationException ex)
            {
                // Assert
                Assert.AreEqual(ex.Response.StatusCode, HttpStatusCode.BadRequest);
            }
        }
    }
}
