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
        const int d = 60;

        int[,] board = new int[x,y];

        public Spel()
        {
            InitializeComponent();

            this.ClientSize = new Size(x * d + 2 * d, y * d + 3 * d);
            this.Paint += paintBoard;
            this.Paint += paintPieces;
            this.Paint += paintScore;

            newGame();
        }



        private void newGame()
        {
            int centerX = x / 2;
            int centerY = y / 2;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    board[i, j] = 0;
                }
            }

            board[centerX, centerY] = 1;
            board[centerX, centerY - 1] = 2;
            board[centerX - 1, centerY] = 2;
            board[centerX - 1, centerY - 1] = 1;
        }

        private void paintBoard(object sender, PaintEventArgs pea)
        {
            Graphics g = pea.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for (int i = 0; i <= x; i++)
                g.DrawLine(Pens.Black, i * d + d, d, i * d + d, y * d + d);

            for (int i = 0; i <= y; i++)
                g.DrawLine(Pens.Black, d, i * d + d, x * d + d, i * d + d);           
        }

        private void paintPieces(object sender, PaintEventArgs pea)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if(board[i, j] != 0)
                        paintPiece(i, j, board[i, j], pea.Graphics);
                }
            }
        }

        private void paintPiece(int col, int row, int player, Graphics g)
        {
            Brush pieceColor = Brushes.White;
            Rectangle field;
            int fieldX, fieldY;

            if (player == 1)
                pieceColor = Brushes.Blue;
            if (player == 2)
                pieceColor = Brushes.Red;

            fieldX = col * d + d;
            fieldY = row * d + d;
            field = new Rectangle(fieldX + 1, fieldY + 1, d - 2, d - 2);

            g.FillEllipse(pieceColor, field);
        }

        private void paintScore(object sender, PaintEventArgs pea)
        {
            Rectangle pieceRed, pieceBlue;
            Graphics g = pea.Graphics;
            int fontSize = 12;
            Font f = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);

            pieceRed = new Rectangle(d, y * d + d + 11, d - 2, d - 2);
            pieceBlue = new Rectangle((x - 1) * d + d, y * d + d + 11, d - 2, d - 2);
            
            g.FillEllipse(Brushes.Red, pieceRed);
            g.FillEllipse(Brushes.Blue, pieceBlue);

            g.DrawString(calculateScore(1).ToString(), f, Brushes.White, d + d / 2 - 8, y * d + d + 11 + d / 2 - 12);
            g.DrawString(calculateScore(2).ToString(), f, Brushes.White, (x - 1) * d + d + d / 2 - 8, y * d + d + 11 + d / 2 - 12);
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
    }
}
