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
        pass,
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
        const int d = 60;
        #endregion

        #region Member variables
        private int activePlayer;
        private int passCounter;
        private bool pressedHelp;
        private Status state;
        private int[,] board;
        private int[,] validLocations;
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
            // Draw white tiles
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if ( (i+(j%2))%2 == 0)
                        g.FillRectangle(Brushes.White, i * d + d, j * d + d, d, d);
                }
            }

            // Draw lines
            for (int i = 0; i <= x; i++)
                g.DrawLine(Pens.Black, i * d + d, d, i * d + d, y * d + d);

            for (int i = 0; i <= y; i++)
                g.DrawLine(Pens.Black, d, i * d + d, x * d + d, i * d + d);
        }

        private void paintPieces(Graphics g, int circleSize)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    // Draw all pieces, 1 = red, 2 = blue
                    if (board[i, j] != 0)
                        g.FillEllipse(board[i, j] == 1 ? Brushes.Red : Brushes.Blue, i * d + d + 1, j * d + d + 1, circleSize, circleSize);
                    // If 'Help' is pressed, show all possible locations for a new piece
                    else if (pressedHelp && validLocations[i, j] == 1)
                        g.FillEllipse(Brushes.Yellow, i * d + d + 1, j * d + d + 1, circleSize, circleSize);
                }
            }

            // Reset pressedHelp to false
            if (pressedHelp)
                pressedHelp = false;
        }

        private void paintScore(Graphics g, StringFormat sf, Font f,  int circleSize)
        {
            Rectangle rect1, rect2;

            rect1 = new Rectangle(d, y * d + 2 * d, circleSize, circleSize);
            rect2 = new Rectangle(x * d, y * d + 2 * d, circleSize, circleSize);

            // Draw two circles with the score in it
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
            else if (state == Status.pass)
                reportState = "Click to pass";
            else if (state == Status.winblue)
                reportState = "Blue wins!";
            else if (state == Status.winred)
                reportState = "Red wins!";
            else if (state == Status.remise)
                reportState = "It's a tie!";

            g.DrawString(reportState, f, Brushes.Black, rect, sf);
        }
        #endregion

        #region Gameplay functions
        private void processClick(object sender, MouseEventArgs mea)
        {
            /* Get the coordinates of the mouseclick, we're doing some complicated casting to avoid clicks directly left to the board to
             * being interpreted as 0 (and thus the first column) instead of -1 */
            int coordX = (int)Math.Floor((double)(mea.X - d) / d);
            int coordY = (int)Math.Floor((double)(mea.Y - d) / d);
            int opponent = activePlayer == 1 ? 2 : 1;

            // Switch turns if the player needed to pass
            if (state == Status.pass)
                switchTurns();
            else
                addPiece(coordX, coordY, validLocations, opponent);
        }

        private void addPiece(int coordX, int coordY, int[,] validLocations, int opponent)
        {
            // First check if the location of the click is within bounds, then if it's a valid location
            if (isWithinBounds(coordX, coordY) && validLocations[coordX, coordY] == 1)
            {
                passCounter = 0;
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

        // Get valid locations fills an array that is used to determine if a click is valid AND counts the number of valid locations
        private int getValidLocations()
        {
            int numberOfValidLocations = 0;
            int opponent = activePlayer == 1 ? 2 : 1;

            // Reinitialise array
            validLocations = new int[x, y];

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (isValidLocation(i, j, opponent))
                    {
                        validLocations[i, j] = 1;
                        numberOfValidLocations++;
                    }
                }
            }

            return numberOfValidLocations;
        }
        
        /* Flip() determines if a location is valid by checking the requirements (one or more pieces of the opponent followed by a piece 
         * of the active player) in all 8 directions. If the parameter 'flip' is set to true it will also flip the piece of the opponent.
         * We decided to use one method for both actions, because otherwise we had to write two methods that would be very much alike */
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

            // Try all directions for the current location
            foreach (int[] operation in operations)
            {
                for (int i = 0, localX = coordX + operation[0], localY = coordY + operation[1];
                    isWithinBounds(localX, localY);
                    i++, localX += operation[0], localY += operation[1])
                {
                    // Next code snippet is only for the first piece in any direction
                    if (i == 0)
                    {
                        // Stop checking in this direction if the piece adjacent to the clicked field is NOT an opponent
                        if (board[localX, localY] != opponent)
                            break;
                    }
                    // All remaining pieces in any direction
                    else
                    {
                        // Stop checking in this direction if there is no piece at this location
                        if (board[localX, localY] == 0)
                            break;
                        // Finding a piece of the active player after one or more of the opponent means the location is valid
                        else if (board[localX, localY] == activePlayer)
                        {
                            // Flip the pieces if function is called with flip = true, otherwise return true
                            if (flip)
                            {
                                // Flip the pieces from end --> start
                                for (int j = i; j >= 0; j--, localX -= operation[0], localY -= operation[1])
                                    board[localX, localY] = activePlayer;
                                break;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        // If an opponent's piece is found we'll just keep looking in the current direction, searching for one of the active player
                    }
                }
            }

            // If all directions are checked and none did satisfy our requirements we'll return false at last
            return false;
        }

        // Sets and resets all variables necessary for a new game
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
            passCounter = 0;

            getValidLocations();

            if (firstGame)
                state = Status.welcome;
            else
                state = Status.newgame;
        }

        private void switchTurns()
        {
            // Switch between players
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

            // Calculate valid locations before start of turn, if there are no valid locations player needs to pass or... 
            if (getValidLocations() == 0)
            {
                state = Status.pass;
                passCounter++;
            }

            // .. if both players need to pass directly after eachother or the board is full, calculate winner
            if (passCounter == 2 || calculateScore(0) == 0)
            {
                int red, blue;
                if ((red = calculateScore(1)) > (blue = calculateScore(2)))
                    state = Status.winred;
                else if (red < blue)
                    state = Status.winblue;
                else
                    state = Status.remise;
            }

            this.Invalidate();
        }

        // Calculate score by walking over the complete board
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

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion
    }
}