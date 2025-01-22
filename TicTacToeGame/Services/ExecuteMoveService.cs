using System;
using System.Collections;
using System.Collections.Generic;

namespace TicTacToeGame.Services
{
    /// <summary>
    /// A Service used to implement tic-tac-toe hueristics logic
    /// </summary>
    public class ExecuteMoveService
    {
        /// <summary>
        /// ExecuteMoveService constructor
        /// </summary>
        public ExecuteMoveService() { }

        /// <summary>
        /// Makes a move for the Azure Player: the hueristics is purely a 0 to board.length - 1 for loop 
        /// </summary>
        /// <param name="gameboard"></param>
        public void EasyMove(string[] gameboard)
        {
            for (int i = 0; i < gameboard.Length; i++)
            {
                if (gameboard[i].Equals("?"))
                {
                    gameboard[i] = "X";
                    break;
                }
            }
        }

        /// <summary>
        /// Check the board to determine available slots for the win
        /// </summary>
        /// <param name="gameboard"></param>
        /// <returns>A List of positions </returns>
        public List<int> DetermineWinPositions(string[] gameboard)
        {
            
            if (gameboard.Length < 9)
            {
                return null;
            }

            if (!gameboard[0].Equals("?") && gameboard[0].Equals(gameboard[1]) && gameboard[1].Equals(gameboard[2]))
            {
                return new List<int> { 0 , 1, 2 };
            }

            if (!gameboard[0].Equals("?") && gameboard[0].Equals(gameboard[3]) && gameboard[3].Equals(gameboard[6]))
            {
                return new List<int> { 0, 3, 6 };
            }

            if (!gameboard[0].Equals("?") && gameboard[0].Equals(gameboard[4]) && gameboard[4].Equals(gameboard[8]))
            {
                return new List<int> { 0, 4, 8 };
            }

            if (!gameboard[1].Equals("?") && gameboard[1].Equals(gameboard[4]) && gameboard[4].Equals(gameboard[7]))
            {
                return new List<int> { 1, 4, 7 };
            }

            if (!gameboard[2].Equals("?") && gameboard[2].Equals(gameboard[4]) && gameboard[4].Equals(gameboard[6]))
            {
                return new List<int> { 2, 4, 6 };
            }

            if (!gameboard[2].Equals("?") && gameboard[2].Equals(gameboard[5]) && gameboard[5].Equals(gameboard[8]))
            {
                return new List<int> { 2, 5, 8 };
            }

            if (!gameboard[3].Equals("?") && gameboard[3].Equals(gameboard[4]) && gameboard[4].Equals(gameboard[5]))
            {
                return new List<int> { 3, 4, 5 };
            }

            if (!gameboard[6].Equals("?") && gameboard[6].Equals(gameboard[7]) && gameboard[7].Equals(gameboard[8]))
            {
                return new List<int> { 6, 7, 8 };
            }

            return null;
        }

        /// <summary>
        /// Checks for a tie, the assumption here is that previous status have been checked
        /// And other validation has been enforces so this checks to see if the board is full
        /// Of Characters other than ?
        /// </summary>
        /// <param name="gameboard"></param>
        /// <returns></returns>
        public bool IsTie(string[] gameboard)
        {
            int count = 0;
            for (int i = 0; i < gameboard.Length; i++)
            {
                if (gameboard[i].Equals("?")) count++;
            }
            return count == 0;
        }
    }
}
