using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace OnlineRPGServerV1
{
    public class Player
    {
        private String name;
        private int id;
        private double x, y, dirX, dirY, targetX, targetY, range, attackAngle, attackSpeed, moveSpeed, attackDamage, health;
        private bool attacking, isAlive;

        private DateTime lastReceived;
        private long attackMS;

        private object mutex = new object();
        private TcpClient socket;
        private NetworkStream stream;

        private Game game;

        public Player(TcpClient socket, int id)
        {
            this.socket = socket;
            this.id = id;

            stream = socket.GetStream();

            Random rnd = new Random();

            x = rnd.NextDouble() * 450 + 50;
            y = rnd.NextDouble() * 450 + 50;
            dirX = 0;
            dirY = 0;
            targetX = x;
            targetY = y;
            range = 120;
            attackAngle = Math.PI / 3.0;
            attackSpeed = 1.0;
            moveSpeed = 120;
            attackDamage = 40;
            health = 100;
            attacking = false;
            isAlive = true;
        }

        private void Update(String json)
        {
            dynamic obj = JObject.Parse(json);

            dirX = obj.dirx;
            dirY = obj.diry;
            targetX = obj.targetx;
            targetY = obj.targety;
            attacking = obj.attacking;
        }

        public void Run()
        {
            try
            {
                dynamic nameInfo = JObject.Parse(Receive());
                name = nameInfo.playername.ToString();

                Console.WriteLine("Player '" + name + "' joined.");

                String gameInfos = GetGameInfos();

                Send(gameInfos);

                dynamic selectedGameInfo = JObject.Parse(Receive());
                int selectedGameID = selectedGameInfo.selectedgame;

                game = Program.GameManager.GetGame(selectedGameID);
                id = game.AcceptPlayer(this);

                bool isRunning = true;

                while (isRunning)
                {
                    lock (mutex)
                    {
                        if (!isAlive)
                            isRunning = false;
                    }

                    String msgToSend = game.ToString();

                    Send(msgToSend);

                    String msgReceived = Receive();

                    lock (mutex)
                    {
                        Update(msgReceived);
                    }
                }
            } catch(Exception e)
            {
                Close();
                return;
            }

            Close();
        }

        private String GetGameInfos()
        {
            JSONBuilder json = new JSONBuilder();
            json.Add("playerid", id);
            
            lock (GameManager.mutex)
            {
                List<JSONBuilder> gameInfos = Program.GameManager.GetGameInfos();

                json.Add("gamecount", gameInfos.Count);

                int gameID = 0;
                foreach (JSONBuilder gameInfo in gameInfos)
                {
                    json.Add("game_" + gameID, gameInfo);
                    gameID++;
                }
            }

            return json.ToString();
        }

        public void Move(DateTime now)
        {
            lock (mutex)
            {
                if (Dist(x, y, targetX, targetY) >= moveSpeed)
                {
                    TimeSpan diff = now.Subtract(lastReceived);

                    double secs = diff.Milliseconds / 1000.0 + diff.Seconds;

                    x += secs * moveSpeed * dirX;
                    y += secs * moveSpeed * dirY;
                }
                else
                {
                    dirX = 0;
                    dirY = 0;
                }
            }
        }

        public void CalculateAttackMS(DateTime now)
        {
            lock (mutex)
            {
                if (attacking)
                {
                    TimeSpan diff = now.Subtract(lastReceived);
                    attackMS += diff.Milliseconds + diff.Seconds * 1000;
                }
                else
                {
                    attackMS = 0;
                }
            }
        }

        public void Attack(List<Player> players)
        {
            lock (mutex)
            {
                long requiredMS = (long)Math.Round(attackSpeed * 1000.0);

                if (attackMS < requiredMS)
                    return;

                attackMS -= requiredMS;

                for (int id = 0; id < players.Count; ++id)
                {
                    if (id == this.id)
                        continue;

                    Player player = players[id];
                    double distance = Dist(x, y, player.x, player.y);

                    if (distance <= range)
                        player.AcceptAttack(player, attackDamage);
                }
            }
        }

        public void AcceptAttack(Player player, double damage)
        {
            lock (mutex)
            {
                health -= damage;

                if (health <= 0)
                    isAlive = false;
            }
        }

        public void UpdateLastReceivedTime(DateTime now)
        {
            lock (mutex)
            {
                lastReceived = now;
            }
        }

        private double Dist(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
        }

        private String Receive()
        {
            byte[] msg = new byte[1024];

            stream.Read(msg, 0, msg.Length);

            return Encoding.ASCII.GetString(msg).Trim();
        }

        private void Send(String json)
        {
            byte[] msg = Encoding.ASCII.GetBytes(json);

            stream.Write(msg, 0, msg.Length);
        }

        public override string ToString()
        {
            JSONBuilder json = new JSONBuilder();

            lock (mutex)
            {
                json.Add("name", name);
                json.Add("id", id);

                json.Add("x", x);
                json.Add("y", y);
                json.Add("dirx", dirX);
                json.Add("diry", dirY);
                json.Add("range", range);
                json.Add("attackangle", attackAngle);
                json.Add("attackspeed", attackSpeed);
                json.Add("movespeed", moveSpeed);
                json.Add("attackdamage", attackDamage);
                json.Add("health", health);
                json.Add("isalive", isAlive);
            }

            return json.ToString();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Player))
                return false;

            Player other = (Player)obj;

            return id == other.id;
        }

        public void Close()
        {
            lock (mutex)
            {
                isAlive = false;

                if (stream != null)
                    stream.Close();

                if (socket != null && socket.Connected)
                    socket.Close();

                Console.WriteLine("Player '" + name + "' exited.");
            }
        }

        public bool IsAlive
        {
            get
            {
                bool result = false;

                lock (mutex)
                {
                    result = isAlive;    
                }

                return result;
            }
        }
    }
}
