using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TicTacToeGame.DataTransferObjects;
using TicTacToeGame.Services;

namespace TicTacToeGame.Controllers
{
    /// <summary>
    /// Methods that act as API endpoint for player requests
    /// </summary>
    [Route("api/v1/tictactoe")]
    [Produces("application/json")]
    [ApiController]
    public class TicTacToeGameController : ControllerBase
    {
        /// <summary>
        /// TicTacToe Game Controller
        /// </summary>
        public TicTacToeGameController() { }

        /// <summary>
        /// A test endpoint
        /// </summary>
        /// <returns> A welcome meaage </returns>
        [HttpGet]
        public string GetWelcomeMessage()
        {
            return $"Welcome to tic_tac_toe! [{DateTime.UtcNow.ToLongDateString()} - {DateTime.UtcNow.ToLongTimeString()}]";
        }

        /// <summary>
        /// Submit game request with human move
        /// </summary>
        /// <param name="gameInputPayload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("executemove")]
        [ProducesResponseType(typeof(GameOutputPayload), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(int), StatusCodes.Status400BadRequest)]
        public ActionResult<GameOutputPayload> ExecuteMove([FromBody] GameInputPayload gameInputPayload)
        {
            if (!IsValid(gameInputPayload))
            {
                return BadRequest(4);
            }

            ExecuteMoveService executeMoveService = new ExecuteMoveService();
            string[] board = gameInputPayload.GameBoard.Select(c => c.ToUpper()).ToArray();

            GameOutputPayload output = new GameOutputPayload()
            {
                AzurePlayerSymbol = gameInputPayload.AzurePlayerSymbol,
                HumamPlayerSymbol = gameInputPayload.HumamPlayerSymbol,
                GameBoard = board
            };

            DetermineStatus(gameInputPayload, executeMoveService, board, output);

            if (!output.Winner.Equals("none"))
            {
                return output;
            }

            executeMoveService.EasyMove(board);

            DetermineStatus(gameInputPayload, executeMoveService, board, output);

            return output;
        }

        /// <summary>
        /// A helper method which used executeMoveService to determine the status of the game board
        /// </summary>
        /// <param name="gameInputPayload"></param>
        /// <param name="executeMoveService"></param>
        /// <param name="board"></param>
        /// <param name="output"></param>
        private static void DetermineStatus(GameInputPayload gameInputPayload, ExecuteMoveService executeMoveService, string[] board, GameOutputPayload output)
        {
            if (executeMoveService.DetermineWinPositions(gameInputPayload.GameBoard) != null)
            {
                output.WinPositions = executeMoveService.DetermineWinPositions(gameInputPayload.GameBoard);
                int winIdxs = output.WinPositions[0];
                output.Winner = gameInputPayload.GameBoard[winIdxs];
            }
            // Requirement says to set to null but int is more convenient in my case so -1 is replacement for null
            else if (executeMoveService.IsTie(board))
            {
                output.Move = -1;
                output.Winner = "Tie";
            }
            else
            {
                output.Winner = "none";
            }
        }

        /// <summary>
        /// Validates the request object to enforse minimum rules
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool IsValid(GameInputPayload input)
        {
            // Validate the game board is of appropriate size
            if (input.GameBoard == null || input.GameBoard.Length < 9 || input.GameBoard.Length > 9)
            {
                return false;
            }

            // Validate X and O representation of Azure and Human respectively
            if (!(input.HumamPlayerSymbol.ToUpper().Equals("O") && input.AzurePlayerSymbol.ToUpper().Equals("X")))
            {
                return false;
            }

            // Validate Appropriate characters and count of player symbols is allowed
            List<string> validChars = new List<string>() { "O", "X", "?" };
            //int countX = 0, countY = 0;
            for (int i = 0; i < input.GameBoard.Length; i++)
            {
                if (!validChars.Contains(input.GameBoard[i].ToUpper()))
                {
                    return false;
                }
            }

            return true;
        }

    }
}
