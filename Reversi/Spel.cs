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

            this.ClientSize = new Size(x * d + 2 * d, y * d + 2 * d);
            this.Paint += paintBoard;
            this.Paint += paintPieces;

            newGame();
        }

        private void newGame()
        {
            int centerX = x / 2;
            int centerY = y / 2;

            board[centerX, centerY] = 1;
            board[centerX, centerY - 1] = 2;
            board[centerX - 1, centerY] = 2;
            board[centerX - 1, centerY - 1] = 1;
        }

        private void paintBoard(object sender, PaintEventArgs pea)
        {
            Graphics g = pea.Graphics;

            g.FillRectangle(Brushes.White, d, d, x * d, y * d);

            for (int i = 0; i <= x; i++)
            {
                g.DrawLine(Pens.Black, i * d + d, d, i * d + d, y * d + d);
            }

            for (int i = 0; i <= y; i++)
            {
                g.DrawLine(Pens.Black, d, i * d + d, x * d + d, i * d + d);
            }

            
        }

        private void paintPieces(object sender, PaintEventArgs pea)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    paintPiece(i, j, board[i, j], pea.Graphics);
                }
            }
        }

        private void paintPiece(int col, int row, int player, Graphics g)
        {
            Brush pieceColor;
            Rectangle field;
            int fieldX, fieldY;

            if (player == 1)
                pieceColor = Brushes.Blue;
            else if (player == 2)
                pieceColor = Brushes.Red;
            else
                pieceColor = Brushes.White;

            fieldX = col * d + d;
            fieldY = row * d + d;
            field = new Rectangle(fieldX, fieldY, d, d);

            g.FillEllipse(pieceColor, field);
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            


        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }


    }
}
