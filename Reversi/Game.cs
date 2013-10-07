using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Reversi
{
    public enum Status
    {
        welcome,
        newgame,
        turnblue,
        turnred,
        winblue,
        winred
    }

    public partial class Game : Form
    {
        const int x = 6;
        const int y = 6;
        const int d = 50;
        int currentPlayer;
        Status state;
        int[,] board;

        public Game()
        {
            InitializeComponent();

            this.ClientSize = new Size(x * d + 2 * d, y * d + 4 * d);
            this.Paint += paintBoard;
            this.MouseClick += addPiece;

            newGame();
            state = Status.welcome;
        }

        private void newGame()
        {
            int centerX = x / 2;
            int centerY = y / 2;

            board = new int[x, y];

            board[centerX - 1, centerY - 1] = 1;
            board[centerX - 1, centerY] = 2;
            board[centerX, centerY] = 1;
            board[centerX, centerY - 1] = 2;

            currentPlayer = 1;
            state = Status.newgame;
        }

        private void paintBoard(object sender, PaintEventArgs pea)
        {
            Graphics g = pea.Graphics;
            int circleSize = d - 2;
            g.SmoothingMode = SmoothingMode.AntiAlias;

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
                        g.FillEllipse(board[i, j] == 1 ? Brushes.Red : Brushes.Blue, i * d + d + 1, j * d + d + 1, circleSize, circleSize);
                }
            }

            // Draw score
            Rectangle rect1, rect2;

            Font font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);

            rect1 = new Rectangle(d, y * d + 2 * d, circleSize, circleSize);
            rect2 = new Rectangle(x * d, y * d + 2 * d, circleSize, circleSize);

            StringFormat stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;

            g.FillEllipse(Brushes.Red, rect1);
            g.FillEllipse(Brushes.Blue, rect2);
            g.DrawString(calculateScore(1).ToString(), font, Brushes.White, rect1, stringFormat);
            g.DrawString(calculateScore(2).ToString(), font, Brushes.White, rect2, stringFormat);

            Pen pen = new Pen(Brushes.Yellow, 5);
            g.DrawEllipse(pen, currentPlayer == 1 ? rect1 : rect2);

            // Draw game state
            string reportState = "";
            Rectangle rect3 = new Rectangle(d, y * d + 2 * d, x * d, d);

            if(state == Status.welcome)
                reportState = "Let's play reversi!";
            else if (state == Status.newgame)
                reportState = "Started a new game";
            else if (state == Status.turnblue)
                reportState = "Blue's turn";
            else if (state == Status.turnred)
                reportState = "Red's turn";
            else if (state == Status.winblue)
                reportState = "Blue has won the game!";
            else if (state == Status.winred)
                reportState = "Red has won the game!";

            g.DrawString(reportState, font, Brushes.Black, rect3, stringFormat);
        }

        private void addPiece(object sender, MouseEventArgs mea)
        {
            int coordX = (int)Math.Floor((double)(mea.X - d) / d);
            int coordY = (int)Math.Floor((double)(mea.Y - d) / d);

            if (isValidLocation(coordX, coordY))
            {
                board[coordX, coordY] = currentPlayer;

                if (currentPlayer == 1)
                {
                    currentPlayer = 2;
                    state = Status.turnblue;
                }
                else if (currentPlayer == 2)
                {
                    currentPlayer = 1;
                    state = Status.turnred;
                }

                this.Invalidate();
            }
        }

        private bool isValidLocation(int coordX, int coordY)
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
