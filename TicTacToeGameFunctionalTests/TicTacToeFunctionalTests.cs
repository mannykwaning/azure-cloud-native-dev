using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Rest;
using RestClientSdkLibrary;
using RestClientSdkLibrary.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TicTacToeGameFunctionalTests
{
    /// <summary>
    /// Funtional tests per homework requirement, I used the "Discription"
    /// Decorator to annotate the intention of the test - XML comments were 
    /// Were excluded for this reason
    /// </summary>
    [TestClass]
    public class TicTacToeFunctionalTests
    {
        ServiceClientCredentials serviceClientCredentials;
        RestClientSdkLibraryClient client;

        [TestInitialize]
        public void initialize()
        {
            serviceClientCredentials = new TokenCredentials("FakeTokenValue");

            client = new RestClientSdkLibraryClient(new Uri("https://localhost:5001"), serviceClientCredentials);
        }

        [TestMethod]
        [Description("(●'◡'●) --> Test detection of a winner X")]
        public async Task TestExecuteMoveWinnerX()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 1,
                AzurePlayerSymbol = "X",
                HumamPlayerSymbol = "O",
                GameBoard = new string[] { "X", "O", "?", "X", "O", "?", "X", "?", "?" }
            };

            // Act
            object resultObject = await client.ExecuteMoveAsync(inputPayload);
            GameOutputPayload gameResultPayload = resultObject as GameOutputPayload;

            // Assert
            if (gameResultPayload != null)
            {
                Assert.IsTrue(gameResultPayload.Winner.ToUpper().Equals("X"));
                Assert.IsTrue(gameResultPayload.WinPositions.Contains(0));
                Assert.IsTrue(gameResultPayload.WinPositions.Contains(3));
                Assert.IsTrue(gameResultPayload.WinPositions.Contains(6));
            }
            else
            {
                Assert.Fail("The Response Oject was null");
            }
        }

        [TestMethod]
        [Description("(●'◡'●) --> Test detection of a winner O")]
        public async Task TestExecuteMoveWinnerO()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 4,
                AzurePlayerSymbol = "X",
                HumamPlayerSymbol = "O",
                GameBoard = new string[] { "O", "X", "?", "X", "O", "?", "X", "?", "O" }
            };

            // Act
            object resultObject = await client.ExecuteMoveAsync(inputPayload);
            GameOutputPayload gameResultPayload = resultObject as GameOutputPayload;

            // Assert
            if (gameResultPayload != null)
            {
                Assert.IsTrue(gameResultPayload.Winner.ToUpper().Equals("O"));
                Assert.IsTrue(gameResultPayload.WinPositions.Contains(0));
                Assert.IsTrue(gameResultPayload.WinPositions.Contains(4));
                Assert.IsTrue(gameResultPayload.WinPositions.Contains(8));
            }
            else
            {
                Assert.Fail("The Response Oject was null");
            }
        }

        [TestMethod]
        [Description("(⓿_⓿) --> Test detection of a tie")]
        public async Task TestExecuteMoveTie()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 3,
                AzurePlayerSymbol = "X",
                HumamPlayerSymbol = "O",
                GameBoard = new string[] { "X", "X", "O", "O", "O", "X", "X", "X", "O" }
            };

            // Act
            object resultObject = await client.ExecuteMoveAsync(inputPayload);
            GameOutputPayload gameResultPayload = resultObject as GameOutputPayload;

            // Assert
            if (gameResultPayload != null)
            {
                Assert.IsTrue(gameResultPayload.Winner.Equals("Tie"));
                Assert.IsTrue(gameResultPayload.Move == -1);
                Assert.IsTrue(gameResultPayload.WinPositions == null);
            }
            else
            {
                Assert.Fail("The Response Oject was null");
            }
        }

        [TestMethod]
        [Description("(.❛ᴗ❛.) --> Test No Winner")]
        public async Task TestExecuteMoveInconclusive()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 3,
                AzurePlayerSymbol = "X",
                HumamPlayerSymbol = "O",
                GameBoard = new string[] { "X", "?", "O", "O", "O", "X", "?", "X", "O" }
            };

            // Act
            object resultObject = await client.ExecuteMoveAsync(inputPayload);
            GameOutputPayload gameResultPayload = resultObject as GameOutputPayload;

            // Assert
            if (gameResultPayload != null)
            {
                Assert.IsTrue(gameResultPayload.Winner.Equals("none"));
                Assert.IsTrue(gameResultPayload.WinPositions == null);
            }
            else
            {
                Assert.Fail("The Response Oject was null");
            }
        }

        [TestMethod]
        [Description("(.❛ᴗ❛.) --> Verify that human player is O and Azure is X")]
        public async Task TestHumanAndAzurePlayerOX()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 3,
                AzurePlayerSymbol = "X",
                HumamPlayerSymbol = "O",
                GameBoard = new string[] { "X", "?", "O", "?", "O", "X", "?", "X", "O" }
            };

            // Act
            HttpOperationResponse<object> resultObject = await client.ExecuteMoveWithHttpMessagesAsync(inputPayload);

            // Assert
            Assert.AreEqual(StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            GameOutputPayload gameResultPayload = resultObject.Body as GameOutputPayload;
            Assert.IsTrue(gameResultPayload.AzurePlayerSymbol.Equals("X"));
            Assert.IsTrue(gameResultPayload.HumamPlayerSymbol.Equals("O"));

        }

        [TestMethod]
        [Description("ψ(._. )> --> Test falied request when human and azure players are swapped")]
        public async Task TestHumanAndAzurePlayerSwap()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 3,
                AzurePlayerSymbol = "O",
                HumamPlayerSymbol = "X",
                GameBoard = new string[] { "X", "?", "O", "O", "O", "X", "?", "X", "O" }
            };

            // Act
            HttpOperationResponse<object> resultObject = await client.ExecuteMoveWithHttpMessagesAsync(inputPayload);

            // Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            int? resultInt = resultObject.Body as int?;
            Assert.IsNotNull(resultInt);
            Assert.AreEqual(4, resultInt.Value);

        }

        [TestMethod]
        [Description("ψ(._. )> --> Test Bad Request when invalid character in array")]
        public async Task TestInvalidCharacterInArray()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 3,
                AzurePlayerSymbol = "O",
                HumamPlayerSymbol = "X",
                GameBoard = new string[] { "X", "?", "B", "O", "O", "X", "?", "X", "O" }
            };

            // Act
            HttpOperationResponse<object> resultObject = await client.ExecuteMoveWithHttpMessagesAsync(inputPayload);

            // Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            int? resultInt = resultObject.Body as int?;
            Assert.IsNotNull(resultInt);
            Assert.AreEqual(4, resultInt.Value);

        }

        [TestMethod]
        [Description("ψ(._. )> --> Test Bad Request when game board is not size 9")]
        public async Task TestInvalidGameBoard()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 3,
                AzurePlayerSymbol = "O",
                HumamPlayerSymbol = "X",
                GameBoard = new string[] { "X", "?", "B", "O", "O", "X" }
            };

            // Act
            HttpOperationResponse<object> resultObject = await client.ExecuteMoveWithHttpMessagesAsync(inputPayload);

            // Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            int? resultInt = resultObject.Body as int?;
            Assert.IsNotNull(resultInt);
            Assert.AreEqual(4, resultInt.Value);

        }

        [TestMethod]
        [Description("ψ(._. )> --> Test Bad Request when game board is empty")]
        public async Task TestInvalidGameBoardIsEmpty()
        {
            // Arrange
            GameInputPayload inputPayload = new GameInputPayload()
            {
                Move = 3,
                AzurePlayerSymbol = "O",
                HumamPlayerSymbol = "X",
                GameBoard = new string[] { }
            };

            // Act
            HttpOperationResponse<object> resultObject = await client.ExecuteMoveWithHttpMessagesAsync(inputPayload);

            // Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            int? resultInt = resultObject.Body as int?;
            Assert.IsNotNull(resultInt);
            Assert.AreEqual(4, resultInt.Value);

        }
    }
}
