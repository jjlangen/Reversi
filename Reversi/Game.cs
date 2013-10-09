using System;
using System.Diagnostics;
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
        winred,
        invalid
    }

    public partial class Game : Form
    {
        // Constants x and y affect rows and colums on the board, d is the field size
        const int x = 6, y = 6, d = 50;
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

            if (state == Status.welcome)
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
            else if (state == Status.invalid)
                reportState = "Invalid move";

            g.DrawString(reportState, font, Brushes.Black, rect3, stringFormat);
        }

        private void addPiece(object sender, MouseEventArgs mea)
        {
            int coordX = (int)Math.Floor((double)(mea.X - d) / d);
            int coordY = (int)Math.Floor((double)(mea.Y - d) / d);

            if (isWithinBounds(coordX, coordY) && isMoveValid(coordX, coordY, currentPlayer))
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
            }

            this.Invalidate();
        }

        private bool isMoveValid(int coordX, int coordY, int currentPlayer)
        {
            int tempX, tempY, counter = 0, result = 0;
            int enemyPlayer = (currentPlayer == 1) ? 2 : 1;
            Point[] replaceLocations = new Point[50];
            string d1 = "", d2 = "", d3 = "";

            // Abort if the position is already taken
            if (board[coordX, coordY] != 0)
                return false;

            // Calculate the relative location of adjecent enemy pieces
            for (int localx = -1; localx <= 1; localx++)
            {
                for (int localy = -1; localy <= 1; localy++)
                {
                    if (isWithinBounds(coordX + localx, coordY + localy))
                    {
                        tempX = localx;
                        tempY = localy;

                        while (nextPiece(coordX, coordY, tempX, tempY) == enemyPlayer)
                        {
                            replaceLocations[counter] = new Point(coordX + tempX, coordY + tempY);
                            tempX += localx;
                            tempY += localy;
                            Debug.WriteLine("replaceLoc[" + counter+ "]: " + replaceLocations[counter]);
                            counter++;
                        }
                        Debug.Write("nextPiece: " + nextPiece(coordX, coordY, tempX, tempY) + " ");
                        Debug.WriteLine("coordX, coordY, tempX, tempY: " + coordX + " " + coordY + " " + tempX + " " + tempY);
                        if(isWithinBounds(coordX + tempX, coordY + tempY) && counter != 0)
                        {
                            if (board[coordX + tempX, coordY + tempY] == currentPlayer)
                            {
                                for (int h = 0; h < counter; h++)
                                {
                                    board[replaceLocations[h].X, replaceLocations[h].Y] = currentPlayer;
                                }

                                result++;
                            }
                        }
                        d1 += counter;
                        counter = 0;
                    }
                }
            }
            Debug.WriteLine("Counter: " + d1);
            if (result == 0)
            {
                state = Status.invalid;
                return false;
            }
            else            
                return true;
        }

        private void changePieces()
        {
        }

        private int nextPiece(int coordX, int coordY, int relativeX, int relativeY)
        {
            int resx = coordX + relativeX;
            int resy = coordY + relativeY;

            if (isWithinBounds(resx, resy))
                return board[resx, resy];
            else
                return 0;
        }

        // Checks if the given field is within the boundaries of the board
        private static bool isWithinBounds(int xx, int yy)
        {
            return xx >= 0 && xx < x && yy >= 0 && yy < y;
        }

        private int calculateScore(int player)
        {
            int res = 0;

            foreach (int i in board)
                if (i == player)
                    res++;

            return res;
        }

        #region Eventhandlers
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newGame();
            this.Invalidate();
        }
        #endregion
    }
}
