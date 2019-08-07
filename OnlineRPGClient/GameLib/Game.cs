using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OnlineRPGClient.GameLib
{
    public class Game
    {
        private String name;
        private bool isRunning;
        private int id, playerNumber;
        private String playerName;
        private int playerID;

        private List<Player> players;

        public Game(GameProxy gameProxy, String playerName, int playerID)
        {
            this.name = gameProxy.Name;
            this.id = gameProxy.ID;
            this.playerNumber = 0;
            this.playerName = playerName;
            this.playerID = playerID;
            this.isRunning = true;
        }

        public void Update(String json)
        {
            dynamic obj = JObject.Parse(json);

            int playerNumber = obj.playernumber;
            isRunning = true;

            if (this.playerNumber != playerNumber)
            {
                this.playerNumber = playerNumber;
                createPlayers(json);
            }
            else
            {
                updatePlayers(json);
            }
        }

        private void updatePlayers(String gameJSON)
        {
            dynamic obj = JObject.Parse(gameJSON);

            foreach (var player in players)
                player.Update(obj["player_" + player.ID].ToString());
        }

        private void createPlayers(String gameJSON)
        {
            dynamic obj = JObject.Parse(gameJSON);

            players = new List<Player>();
            for (int i = 0; i < playerNumber; ++i)
                players.Add(new Player(obj["player_" + i].ToString()));
        }

        public String Name => name;
        public int ID => id;
        public bool IsRunning => isRunning;
        public int PlayerNumber => playerNumber;
        public String PlayerName => playerName;
        public int PlayerID => playerID;
        public List<Player> Players => players;

        public Player Player => players[playerID];
    }
}
