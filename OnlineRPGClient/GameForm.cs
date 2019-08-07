using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using OnlineRPGClient.GameLib;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace OnlineRPGClient
{
    public partial class GameForm : Form
    {
        Socket socket;
        String playerName;
        int playerID;
        Game game;

        Queue<long> times = new Queue<long>(100);

        Thread socketThread;

        private readonly object gameLock = new object();
        private readonly object timesLock = new object();

        public GameForm()
        {
            InitializeComponent();
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            Login login = new Login();

            if (login.ShowDialog() == DialogResult.OK)
            {
                playerName = login.Name;
            }
            else
            {
                MessageBox.Show(this, "You have to login", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            try
            {
                Connect();

                dynamic gameInfos = JObject.Parse(SendNameAndGetGameInfos(playerName));

                List<GameProxy> games = new List<GameProxy>();

                int gameCount = gameInfos.gamecount;
                playerID = gameInfos.playerid;

                for (int i = 0; i < gameCount; ++i)
                {
                    games.Add(new GameProxy(gameInfos["game_" + i].ToString()));
                }

                GameSelect gameSelect = new GameSelect(games);

                gameSelect.ShowDialog();

                GameProxy selectedGame = gameSelect.Game;

                game = new Game(selectedGame, playerName, playerID);

                String gameInfo = SendSelectedGameAndGetGameInfo(game.ID);

                game.Update(gameInfo);

                socketThread = new Thread(new ThreadStart(SocketThread));
                socketThread.Priority = ThreadPriority.AboveNormal;
                socketThread.Start();

                timer1.Start();

                Show();

                MessageBox.Show("It started!");
            }
            catch (SocketException ex)
            {
                MessageBox.Show(this, ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void Connect()
        {
            IPHostEntry host = Dns.GetHostEntry("127.0.0.1");
            IPAddress address = host.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(address, 6543);

            socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endPoint);
        }

        private String SendNameAndGetGameInfos(String name)
        {
            String nameJSON = "{ 'playername':'" + name + "'}";
            Send(nameJSON);
            return Receive();
        }

        private String SendSelectedGameAndGetGameInfo(int gameID)
        {
            String selectedGameJSON = "{ 'selectedgame':" + gameID + "}";
            Send(selectedGameJSON);
            return Receive();
        }

        private bool Send(String json)
        {
            byte[] msg = Encoding.ASCII.GetBytes(json);

            socket.Send(msg);

            return true;
        }

        private String Receive()
        {
            byte[] msg = new byte[10240];

            socket.Receive(msg);

            String json = Encoding.ASCII.GetString(msg).Trim();

            return json;
        }

        private void SocketThread()
        {
            bool isRunning = true;

            while (isRunning)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                String playerData;

                lock (gameLock)
                {
                    // game.Player.SetMouse(Cursor.Position.X, Cursor.Position.Y);
                    playerData = game.Player.ToString();
                }

                Send(playerData);
                String gameInfo = Receive();

                lock (gameLock)
                { 
                    game.Update(gameInfo);
                    isRunning = game.IsRunning;
                }

                stopwatch.Stop();

                lock (timesLock)
                {
                    times.Enqueue(stopwatch.ElapsedMilliseconds);
                    if (times.Count > 100)
                        times.Dequeue();
                }

                Thread.Sleep(5);
            }
        }

        public double GetAverageDelay()
        {
            double avg = 0;
            lock (timesLock)
            {
                foreach (long time in times)
                    avg += (double)time;

                if (times.Count > 0)
                    avg /= (double)times.Count;
            }

            return avg;
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (socketThread != null && socketThread.IsAlive)
            {
                socketThread.Abort();
            }

            if (socket != null && socket.Connected)
            {
                socket.Close();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {

            listBox1.Items.Clear();

            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height, PixelFormat.Format32bppArgb);

            Graphics graphics = Graphics.FromImage(bitmap);
            Brush greenBrush = new SolidBrush(Color.Green);
            Brush redBrush = new SolidBrush(Color.Red);

            lock (gameLock)
            {
                foreach (Player player in game.Players)
                {
                    listBox1.Items.Add(player.Name + "\t" + player.Health);

                    Rectangle rect = new Rectangle((int)Math.Round(player.X) - 20, (int)Math.Round(player.Y) - 20, 40, 40);

                    if (playerID == player.ID)
                        graphics.FillEllipse(greenBrush, rect);
                    else
                        graphics.FillEllipse(redBrush, rect);

                    graphics.DrawLine(new Pen(redBrush), (int)Math.Round(player.X), (int)Math.Round(player.Y), (int)(Math.Round(player.X) + player.DirX * player.MoveSpeed), (int)(Math.Round(player.Y) + player.DirY * player.MoveSpeed));
                }
            }

            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }

            pictureBox1.Image = bitmap;

            Refresh();

            Text = "Game - " + GetAverageDelay() + " ms";
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                lock (gameLock)
                {
                    game.Player.SetMouse(e.X, e.Y);
                }
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                lock (gameLock)
                {
                    game.Player.Attacking = true;
                }
            }
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                lock (gameLock)
                {
                    game.Player.Attacking = false;
                }
            }
        }
    }
}
