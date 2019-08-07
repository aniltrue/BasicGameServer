using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineRPGServerV1
{
    public class Game
    {
        public object mutex = new object();

        private List<Player> players;

        private int id;
        private String name;
        private bool isAlive;

        public Game(int id, String name)
        {
            this.id = id;
            this.name = name;
            isAlive = true;
            players = new List<Player>();
        }

        public void Run()
        {
            lock (mutex)
            {
                Console.WriteLine(name + " game is started.");
            }

            try
            {
                bool isRunning = isAlive;

                while (isRunning)
                {
                    lock (mutex)
                    {
                        DateTime now = DateTime.Now;

                        foreach (Player player in players)
                        {
                            if (player.IsAlive)
                                player.Move(now);
                        }

                        foreach (Player player in players)
                        {
                            if (player.IsAlive)
                                player.CalculateAttackMS(now);
                        }

                        foreach (Player player in players)
                        {
                            if (player.IsAlive)
                                player.Attack(players);
                        }

                        foreach (Player player in players)
                        {
                            if (player.IsAlive)
                                player.UpdateLastReceivedTime(now);
                        }

                        if (players.Count > 0)
                        {
                            foreach (Player player in players)
                            {
                                if (!player.IsAlive)
                                {
                                    isAlive = false;
                                    break;
                                }

                            }
                        }

                        isRunning = isAlive;
                    }

                    Thread.Sleep(5);
                }
            } catch (Exception e)
            {
                Close();
                return;
            }

            Close();
        }

        public int AcceptPlayer(Player player)
        {
            int id = 0;

            lock (mutex)
            {
                players.Add(player);
                id = players.Count - 1;
            }

            return id;
        }

        public int ID => id;

        public String Name => name;

        public bool IsAlive
        {
            get
            {
                lock (mutex)
                {
                    return isAlive;
                }
            }
        }

        public JSONBuilder GetProxyJSON()
        {
            JSONBuilder json = new JSONBuilder();

            lock (mutex)
            {
                json.Add("name", name);
                json.Add("id", id);
                json.Add("players", players.Count);
            }

            return json;
        }

        public override String ToString()
        {
            JSONBuilder json = new JSONBuilder();

            lock (mutex)
            {
                json.Add("name", name);
                json.Add("id", id);
                json.Add("playernumber", players.Count);

                for (int id = 0; id < players.Count; ++id)
                {
                    Player player = players[id];
                    json.Add("player_" + id, player);
                }
            }

            return json.ToString();
        }

        public void Close()
        {
            lock (mutex)
            {
                isAlive = false;

                Program.GameManager.RemoveGame(this);

                Console.WriteLine(name + " game is end.");
            }
        }
    }
}
