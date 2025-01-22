using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TicTacToeGameUnitTests
{
    [TestClass()]
    public class ExecuteMoveServiceUnitTests
    {

        [TestMethod()]
        public void ExecuteMoveServiceTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void EasyMoveTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        [Description("😊 --> Test Game Won by Human (X) on 0, 1, 2")]
        public void DetermineWinPositionsTest()
        {
            // Arrange
            string[] gameboard = new string[]{ "X", "X", "X", "?", "?", "?", "?", "O", "?" };
            
            // Act

            // Assert
            Assert.Fail();
        }

        [TestMethod()]
        public void IsWinnerTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IsTieTest()
        {
            Assert.Fail();
        }
    }
}