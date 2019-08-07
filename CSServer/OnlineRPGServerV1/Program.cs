using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace OnlineRPGServerV1
{
    public class Program
    {
        public static GameManager GameManager;
        public static int playerCounter;

        public static void Main(string[] args)
        {
            Console.Title = "Server";

            GameManager = new GameManager();
            GameManager.AddGame(new Game(0, "myGame"));

            playerCounter = 0;

            Thread thread = new Thread(new ThreadStart(Run));
            thread.Start();

            Console.ReadLine();
        }

        private static void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 6543);
            listener.Start();

            Console.WriteLine(">>Server Started");

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();

                Player player = new Player(socket, playerCounter);
                playerCounter++;

                Thread thread = new Thread(new ThreadStart(player.Run));
                thread.Start();
            }

            listener.Stop();

            Console.WriteLine(">>Server Closed");
        }
    }
}
