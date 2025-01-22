﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace RestClientSdkLibrary.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;

    public partial class GameOutputPayload
    {
        /// <summary>
        /// Initializes a new instance of the GameOutputPayload class.
        /// </summary>
        public GameOutputPayload() { }

        /// <summary>
        /// Initializes a new instance of the GameOutputPayload class.
        /// </summary>
        public GameOutputPayload(int? move = default(int?), string azurePlayerSymbol = default(string), string humamPlayerSymbol = default(string), string winner = default(string), IList<int?> winPositions = default(IList<int?>), IList<string> gameBoard = default(IList<string>))
        {
            Move = move;
            AzurePlayerSymbol = azurePlayerSymbol;
            HumamPlayerSymbol = humamPlayerSymbol;
            Winner = winner;
            WinPositions = winPositions;
            GameBoard = gameBoard;
        }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "move")]
        public int? Move { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "azurePlayerSymbol")]
        public string AzurePlayerSymbol { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "humamPlayerSymbol")]
        public string HumamPlayerSymbol { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "winner")]
        public string Winner { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "winPositions")]
        public IList<int?> WinPositions { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "gameBoard")]
        public IList<string> GameBoard { get; set; }

    }
}
