using System.Collections.Generic;

namespace TicTacToeGame.DataTransferObjects
{
    /// <summary>
    /// Game Output payload
    /// </summary>
    public class GameOutputPayload
    {
        public int Move { get; set; }

        public string AzurePlayerSymbol { get; set; }

        public string HumamPlayerSymbol { get; set; }

        public string Winner { get; set; }

        public List<int> WinPositions { get; set; }

        public string[] GameBoard { get; set; }
    }
}
