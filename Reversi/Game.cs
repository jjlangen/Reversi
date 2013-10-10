using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Reversi
{
    #region Enumerations
    public enum Status
    {
        welcome,
        newgame,
        turnblue,
        turnred,
        winblue,
        winred,
        remise
    }
    #endregion

    public partial class Game : Form
    {
        #region Setup
        // Constants x and y affect rows and colums on the board, d is the field size
        const int x = 6;
        const int y = 6;
        const int d = 50;
        #endregion

        #region Globals
        int activePlayer;
        bool pressedHelp;
        Status state;
        int[,] board;
        #endregion

        #region Constructor
        public Game()
        {
            InitializeComponent();

            this.ClientSize = new Size(x * d + 2 * d, y * d + 4 * d);
            this.Paint += paintGUI;
            this.MouseClick += processClick;

            startNewGame(true);
        }
        #endregion

        #region Graphical functions
        private void paintGUI(object sender, PaintEventArgs pea)
        {
            const int circleSize = d - 2;
            Graphics g = pea.Graphics;
            Font font = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);
            StringFormat stringFormat = new StringFormat();

            g.SmoothingMode = SmoothingMode.AntiAlias;
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Center;
            
            paintBoard(g, circleSize);
            paintPieces(g, circleSize);
            paintScore(g, stringFormat, font, circleSize);
            paintState(g, stringFormat, font);
        }

        private void paintBoard(Graphics g, int circleSize)
        {
            for (int i = 0; i <= x; i++)
                g.DrawLine(Pens.Black, i * d + d, d, i * d + d, y * d + d);

            for (int i = 0; i <= y; i++)
                g.DrawLine(Pens.Black, d, i * d + d, x * d + d, i * d + d);
        }

        private void paintPieces(Graphics g, int circleSize)
        {
            int[,] validLocations = getValidLocations();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (board[i, j] != 0)
                        g.FillEllipse(board[i, j] == 1 ? Brushes.Red : Brushes.Blue, i * d + d + 1, j * d + d + 1, circleSize, circleSize);
                    else if (pressedHelp && validLocations[i, j] == 1)
                        g.FillEllipse(Brushes.Yellow, i * d + d + 1, j * d + d + 1, circleSize, circleSize);
                }
            }

            if (pressedHelp)
                pressedHelp = false;
        }

        private void paintScore(Graphics g, StringFormat sf, Font f,  int circleSize)
        {
            Rectangle rect1, rect2;

            rect1 = new Rectangle(d, y * d + 2 * d, circleSize, circleSize);
            rect2 = new Rectangle(x * d, y * d + 2 * d, circleSize, circleSize);

            g.FillEllipse(Brushes.Red, rect1);
            g.FillEllipse(Brushes.Blue, rect2);
            g.DrawString(calculateScore(1).ToString(), f, Brushes.White, rect1, sf);
            g.DrawString(calculateScore(2).ToString(), f, Brushes.White, rect2, sf);

            // Paint a yellow circle around active player
            g.DrawEllipse(new Pen(Brushes.Yellow, 5), activePlayer == 1 ? rect1 : rect2);
        }

        private void paintState(Graphics g, StringFormat sf, Font f)
        {
            Rectangle rect;
            string reportState = "";

            // A helper rectangle around the entire scoreboard to align the gamestate text
            rect = new Rectangle(d, y * d + 2 * d, x * d, d);

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
            else if (state == Status.remise)
                reportState = "It's a tie!";

            g.DrawString(reportState, f, Brushes.Black, rect, sf);
        }
        #endregion

        #region Gameplay functions
        private void processClick(object sender, MouseEventArgs mea)
        {
            int coordX = (int)Math.Floor((double)(mea.X - d) / d);
            int coordY = (int)Math.Floor((double)(mea.Y - d) / d);
            int opponent = activePlayer == 1 ? 2 : 1;
            int[,] validLocations = getValidLocations();

            addPiece(coordX, coordY, validLocations, opponent);
            // checkScore();
        }

        private void addPiece(int coordX, int coordY, int[,] validLocations, int opponent)
        {
            if (isWithinBounds(coordX, coordY) && validLocations[coordX, coordY] == 1)
            {
                board[coordX, coordY] = activePlayer;
                flip(coordX, coordY, opponent, true);
                switchTurns();
            }
        }

        private bool isValidLocation(int coordX, int coordY, int opponent)
        {
            // If the location is taken, return false
            if (board[coordX, coordY] != 0)
                return false;

            return flip(coordX, coordY, opponent, false);
        }

        private int[,] getValidLocations()
        {
            int[,] validLocations = new int[x, y];
            int opponent = activePlayer == 1 ? 2 : 1;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (isValidLocation(i, j, opponent))
                        validLocations[i, j] = 1;
                }
            }

            return validLocations;
        }

        private bool flip(int coordX, int coordY, int opponent, bool flip)
        {
            // Define possible operations (directions)
            int[][] operations = new int[][] {
            new int[] {0,-1},    // Up
            new int[] {1,-1},    // Up Right
            new int[] {1,0},     // Right
            new int[] {1,1},     // Down Right
            new int[] {0,1},     // Down
            new int[] {-1,1},    // Down left
            new int[] {-1,0},    // Left
            new int[] {-1,-1},   // Up Left
            };
            /* Try all directions for the current location, the first spot we encounter NEEDS to be occupied by the opponent, if we find an empty spot after that => break loop,
             * if we find another spot occupied by opponent => keep digging, if we find a spot occupied by the current player => location is valid */
            foreach (int[] operation in operations)
            {
                for (int i = 0, localX = coordX + operation[0], localY = coordY + operation[1];
                    isWithinBounds(localX, localY);
                    i++, localX += operation[0], localY += operation[1])
                {
                    // Only run the enclosed code on the fields adjacent to the clicked field
                    if (i == 0)
                    {
                        // Stop (and return false) if the piece at this location is NOT an opponent
                        if (board[localX, localY] != opponent)
                            break;
                    }
                    // Run this code when not scanning fields adjacent to the clicked field
                    else
                    {
                        // Stop (and return false) if there is no piece at this location
                        if (board[localX, localY] == 0)
                            break;
                        // Finding a piece of the active player returns a positive result
                        else if (board[localX, localY] == activePlayer)
                        {
                            // Flip the pieces if function is called with flip = true
                            if (flip)
                            {
                                // Flip the pieces from end --> start
                                for (int j = i; j >= 0; j--, localX -= operation[0], localY -= operation[1])
                                    board[localX, localY] = activePlayer;
                                break;
                            }
                            return true;
                        }
                        // If an opponents piece is found it will just try to run the for-loop once more
                    }
                }
            }

            return false;
        }

        private void startNewGame(bool firstGame)
        {
            const int centerX = x / 2;
            const int centerY = y / 2;

            board = new int[x, y];

            board[centerX - 1, centerY - 1] = 1;
            board[centerX - 1, centerY] = 2;
            board[centerX, centerY] = 1;
            board[centerX, centerY - 1] = 2;

            activePlayer = 1;
            pressedHelp = false;

            if (firstGame)
                state = Status.welcome;
            else
                state = Status.newgame;
        }

        private void switchTurns()
        {
            if (activePlayer == 1)
            {
                activePlayer = 2;
                state = Status.turnblue;
            }
            else if (activePlayer == 2)
            {
                activePlayer = 1;
                state = Status.turnred;
            }

            this.Invalidate();
        }

        private int calculateScore(int player)
        {
            int res = 0;

            foreach (int i in board)
                if (i == player)
                    res++;

            return res;
        }

        private static bool isWithinBounds(int xx, int yy)
        {
            return xx >= 0 && xx < x && yy >= 0 && yy < y;
        }
        #endregion

        #region ToolStripMenu Eventhandlers
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startNewGame(false);
            this.Invalidate();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            pressedHelp = true;
            this.Invalidate();
        }
        #endregion
    }
}
