using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OnlineRPGServerV1
{
    public class GameManager
    {
        public static readonly object mutex = new object();

        private List<JSONBuilder> gameInfos;
        private List<Game> games;

        public GameManager()
        {
            gameInfos = new List<JSONBuilder>();
            games = new List<Game>();
        }

        public List<JSONBuilder> GetGameInfos()
        {
            return gameInfos;
        }

        public Game GetGame(int id)
        {
            Game game = null;
            
            lock (mutex)
            {
                foreach (Game g in games)
                {
                    if (g.ID == id)
                    {
                        game = g;
                        break;
                    }
                }
            }

            return game;
        }

        public void AddGame(Game game)
        {
            lock (mutex)
            {
                games.Add(game);
                
                lock (game.mutex)
                {
                    gameInfos.Add(game.GetProxyJSON());
                }
            }

            Thread gameThread = new Thread(new ThreadStart(game.Run));
            gameThread.Start();
        }

        public void RemoveGame(Game game)
        {
            lock (mutex)
            {
                games.Remove(game);

                int id = -1;

                for (int i = 0; i < gameInfos.Count; ++i)
                {
                    JSONBuilder gameInfo = gameInfos[i];
                    if (gameInfo.Clone().ToString().Contains("'id':" + game.ID))
                    {
                        id = i;
                        break;
                    }
                }

                if (id != -1)
                    gameInfos.RemoveAt(id);
            }
        }
    }
}
