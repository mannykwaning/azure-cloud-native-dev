using ImageProducerRestSdk;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace ImageProducerTests
{
    [TestClass]
    class FunctionalTests
    {
        private ServiceClientCredentials _serviceClientCredentials;
        private RestClientSDKLibrary _client;
        private HttpClient httpClient;

        [TestInitialize]
        public void Initialize()
        {
            _serviceClientCredentials = new TokenCredentials("FakeTokenValue");

            _client = new RestClientSDKLibrary("https://localhost:5001/", httpClient);
        }

        [TestMethod("(●'◡'●) Test file was sucessfully uploaded")]
        public async Task TestFileUpload()
        {
            // Arrange
            string fileName = "eminem.jpg";

            using System.IO.FileStream fileStream = System.IO.File.OpenRead(fileName);

            FileParameter file = new FileParameter(fileStream);

            // Act
            string response = await _client.UploadFileAsync("1", file);

            // Assert
            Assert.IsTrue(response.Equals("201"));
        }
    }
}
