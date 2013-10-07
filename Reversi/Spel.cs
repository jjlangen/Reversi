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
        const int d = 50;

        int[,] board;

        int currentPlayer;

        public Spel()
        {
            InitializeComponent();

            this.ClientSize = new Size(x * d + 2 * d, y * d + 4 * d);
            this.Paint += paintBoard;
            this.MouseClick += addPiece;

            newGame();
        }

        private void newGame()
        {
            board = null;
            board = new int[x, y];

            int centerX = x / 2;
            int centerY = y / 2;

            board[centerX - 1, centerY - 1] = 1;
            board[centerX - 1, centerY] = 2;
            board[centerX, centerY] = 1;
            board[centerX, centerY - 1] = 2;

            currentPlayer = 1;
        }

        private void paintBoard(object sender, PaintEventArgs pea)
        {
            Graphics g = pea.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw lines
            for (int i = 0; i <= x; i++)
                g.DrawLine(Pens.Black, i * d + d, d, i * d + d, y * d + d);

            for (int i = 0; i <= y; i++)
                g.DrawLine(Pens.Black, d, i * d + d, x * d + d, i * d + d);

            // Draw pieces
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (board[i, j] != 0)
                        g.FillEllipse(board[i, j] == 1 ? Brushes.Red : Brushes.Blue, i * d + d, j * d + d, d, d);
                }
            }

            // Draw score
            Font font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);

            Rectangle rect1 = new Rectangle(d, y * d + 2 * d, d, d);
            Rectangle rect2 = new Rectangle(x * d, y * d + 2 * d, d, d);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            g.FillEllipse(Brushes.Red, rect1);
            g.FillEllipse(Brushes.Blue, rect2);
            g.DrawString(calculateScore(1).ToString(), font, Brushes.White, rect1, stringFormat);
            g.DrawString(calculateScore(2).ToString(), font, Brushes.White, rect2, stringFormat);

            Pen pen = new Pen(Brushes.Yellow, 5);
            g.DrawEllipse(pen, currentPlayer == 1 ? rect1 : rect2);
        }

        private void addPiece(object sender, MouseEventArgs mea)
        {
            int coordX = (int)Math.Floor((double)(mea.X - d) / d);
            int coordY = (int)Math.Floor((double)(mea.Y - d) / d);

            if (validLocation(coordX, coordY))
            {
                board[coordX, coordY] = currentPlayer;
                currentPlayer = (currentPlayer == 1) ? 2 : 1;
                this.Invalidate();
            }
        }

        private bool validLocation(int coordX, int coordY)
        {
            if (coordX < 0 || coordX >= x || coordY < 0 || coordY >= y)
            {
                return false;
            }

            if (board[coordX, coordY] != 0)
            {
                return false;
            }

            return true;
        }


        private int calculateScore(int player)
        {
            int res = 0;

            foreach (int i in board)
                if (i == player)
                    res++;

            return res;
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newGame();
            this.Invalidate();
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }


    }
}
