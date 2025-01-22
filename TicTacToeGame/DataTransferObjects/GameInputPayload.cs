using System.Collections.Generic;

namespace TicTacToeGame.DataTransferObjects
{
    /// <summary>
    /// Game Imput Payload
    /// </summary>
    public class GameInputPayload
    {
        public int Move { get; set; }

        public string AzurePlayerSymbol { get; set; }

        public string HumamPlayerSymbol { get; set; }

        public string[] GameBoard { get; set; }
    }
}
