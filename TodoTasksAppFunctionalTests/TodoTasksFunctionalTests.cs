using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestClientSdkLibraryTasksApp;
using RestClientSdkLibraryTasksApp.Models;
using System;
using System.Threading.Tasks;

namespace TodoTasksAppFunctionalTests
{
    /// <summary>
    /// Funtional tests per homework extra credit requirement, I used the "Description"
    /// Decorator to annotate the intention of the test - XML comments were 
    /// Were excluded for this reason
    /// </summary>
    [TestClass]
    public class TodoTasksFunctionalTests
    {
        ServiceClientCredentials serviceClientCredentials;
        RestClientSdkLibraryTasksAppClient client;

        [TestInitialize]
        public void initialize()
        {
            serviceClientCredentials = new TokenCredentials("FakeTokenValue");

            client = new RestClientSdkLibraryTasksAppClient(new Uri("https://localhost:5001"), serviceClientCredentials);
        }

        [TestMethod]
        [Description("(●'◡'●) --> Test Get a Task by Id")]
        public async Task TestCreateTask()
        {

            object result = await client.GetTaskByIdAsync(2);
            TodoTaskResult todoTaskResult = result as TodoTaskResult;
            
            // Assert
            Assert.IsTrue(todoTaskResult.TaskName.Equals("Workout"));
            
        }

    }
}
