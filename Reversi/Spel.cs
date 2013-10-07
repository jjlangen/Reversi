using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reversi
{
    public partial class Spel : Form
    {
        const int x = 6;
        const int y = 6;
        int[,] board = new int[x,y];

        public Spel()
        {
            InitializeComponent();

            this.ClientSize = new Size(x * 50 + 100, y * 50 + 50);
            this.Paint += paintBoard;

            newGame();
        }

        private void newGame()
        {
            int centerX = x / 2;
            int centerY = y / 2;

            board[centerX, centerY] = 1;
            board[centerX, centerY + 1] = 2;
            board[centerX + 1, centerY] = 2;
            board[centerX + 1, centerY + 1] = 1;
        }

        private void paintBoard(object sender, PaintEventArgs pea)
        {
            Graphics g = pea.Graphics;

            for (int i = 0; i <= x; i++)
            {
                int d = 50;

                g.DrawLine(Pens.Black, i * 50 + d, d, i * 50 + d, y * 50 + d);
                g.DrawLine(Pens.Black, 50, i * 50 + d, x * 50 + d, i * 50 + d);
            }
        }
 

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            


        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }


    }
}
