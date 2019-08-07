using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OnlineRPGClient
{
    public partial class GameSelect : Form
    {
        private List<GameProxy> games;
        private GameProxy game;

        public GameSelect(List<GameProxy> games)
        {
            InitializeComponent();
            this.games = games;
        }

        public GameProxy Game => game;

        private void GameSelect_Load(object sender, EventArgs e)
        {
            foreach (var game in games)
                listBox1.Items.Add(game);

        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                game = (GameProxy)listBox1.SelectedItem;
                Close();
            }
        }
    }
}
