using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class Game
    {
        /// <summary>
        /// Class member variables
        /// </summary>
        public DiskBoard diskBoard;
        public PegBoard pegBoard;
        private Player player1;
        private Player player2;
        public Player currentPlayer;
        static private int boardSize = 6; // change later?

        public enum TypeOfGame { QuickMatch = 0, Campaign, Network, Local, AI, TimeAttack };

        /// <summary> Game
        /// Public Constructors
        /// (+4) overloads
        /// </summary>
        public Game() // Default 
        {
        }
        public Game(Player me, GameOptions gameOptions) // Campaign, Quickgame, TimeAttack
        {
            diskBoard = new DiskBoard(boardSize);
            pegBoard = new PegBoard(boardSize + 1);

            player1 = me;
            player2 = new Player(gameOptions.player2Name, Player.Color.Black, Player.TypeOfPlayer.AI);

            currentPlayer = player1;
        }
        public Game(Game.TypeOfGame type) // AI v AI
        {
            diskBoard = new DiskBoard(boardSize);
            pegBoard = new PegBoard(boardSize + 1);

            player1 = new Player("AI1", Player.Color.White, Player.TypeOfPlayer.AI);
            player2 = new Player("AI2", Player.Color.Black, Player.TypeOfPlayer.AI);

            currentPlayer = player1;
        }
        public Game(string p1Name, string p2Name, int whichOneIsPlayer1) // Network
        {
            diskBoard = new DiskBoard(boardSize);
            pegBoard = new PegBoard(boardSize + 1);

            if (whichOneIsPlayer1 == 1)
            {
                player1 = new Player(p1Name, Player.Color.White, Player.TypeOfPlayer.Human);
                player2 = new Player(p2Name, Player.Color.Black, Player.TypeOfPlayer.Network);
            }
            else
            {
                player1 = new Player(p1Name, Player.Color.White, Player.TypeOfPlayer.Network);
                player2 = new Player(p2Name, Player.Color.Black, Player.TypeOfPlayer.Human);
            }

            currentPlayer = player1;
        }
        public Game(string p1Name, string p2Name) // Local
        {
            diskBoard = new DiskBoard(boardSize);
            pegBoard = new PegBoard(boardSize + 1);


            player1 = new Player(p1Name, Player.Color.White, Player.TypeOfPlayer.Human);
            player2 = new Player(p2Name, Player.Color.Black, Player.TypeOfPlayer.Human);

            currentPlayer = player1;
        }

        
        /// <summary> isValid
        /// Game Core: check if a move is VALID
        /// (+2) overloads
        /// </summary>
        public bool isValid(Pair to, Pair from)
        {
            // Overloaded for a default argument parameter
            return isValid(to, from, this.pegBoard, this.currentPlayer.color);
        }
        public bool isValid(int row, int col, Pair from)
        {
            return isValid(new Pair(row, col), from);
        }
        private bool isValid(Pair to, Pair from, PegBoard pegBoard, Player.Color currentPlayerColor)
        {
            int moveUpDown = from.row - to.row;
            int moveLeftRight = from.col - to.col;

            if (to.row < 0 || to.col < 0 || from.row < 0 || from.col < 0
                || to.row > boardSize || to.col > boardSize || from.row > boardSize || from.col > boardSize)
            {
                return false;
            }

            else if (pegBoard.checkColor(from) != currentPlayerColor // correct color
                || pegBoard.isEmpty(from)     // selected empty peg slot
                || !pegBoard.isEmpty(to))  // dropped on occupied slot
            {
                // Conditions for immediate failure
                return false;
            }

            else if (Math.Abs(moveUpDown) == 1
                && Math.Abs(moveLeftRight) == 1)
            {
                // Jump
                return true;
            }

            else if (Math.Abs(moveUpDown) + Math.Abs(moveLeftRight) == 1)
            {
                // Move peg over 1
                return true;
            }

            else if (Math.Abs(moveUpDown) + Math.Abs(moveLeftRight) == 2)
            {
                // jump (2,0) or (0,2) --> the (1,1) case has already been tested
                return (!pegBoard.isEmpty(from.row - moveUpDown / 2, from.col - moveLeftRight / 2)
                        && pegBoard.checkColor(from.row - moveUpDown / 2, from.col - moveLeftRight / 2) != currentPlayerColor);
                // Jumps A color and Jumps OTHER color
            }

            else
            {
                // Don't jump the rainbow!
                return false;
            }
        }


        /// <summary> move
        /// Make move and change board state
        /// (+1) overload
        /// </summary>
        public void move(int row, int col, Pair from)
        {
            move(new Pair(row, col), from);
        }
        public void move(Pair to, Pair from)
        {
            pegBoard.movePeg(currentPlayer.color, to, from);

            if ((Math.Abs(to.row-from.row) == 1
                && Math.Abs(to.col-from.col) == 1))
            {
                // Add disk
                diskBoard.placeDisk(currentPlayer, to, from);
            }
        }


        /*/// <summary> moveAI
        /// calls AI and returns (to) and (from)
        /// </summary>
        public PieceMove moveAI()
        {
            PieceMove aiMove = currentPlayer.getAIMove(currentPlayer, pegBoard, diskBoard);
            move(aiMove.to, aiMove.from);
            return aiMove;
        }*/

        /// <summary> moveAI
        /// calls AI and returns (to) and (from)
        /// </summary>
        public PieceMove moveAI(int treeDepth)
        {
            PieceMove aiMove = currentPlayer.getAIMove(currentPlayer, pegBoard, diskBoard, treeDepth);
            
            if(currentPlayer.playerType == Player.TypeOfPlayer.AI)
                move(aiMove.to, aiMove.from);

            return aiMove;
        }


        /// <summary> switchPlayer
        /// self-explanatory
        /// </summary>
        public void switchPlayer()
        {
            if (currentPlayer == player1)
            {
                this.currentPlayer = player2;
            }
            else
            {
                this.currentPlayer = player1;
            }
        }

        public Player.Color otherColor()
        {
            if (currentPlayer.color == Player.Color.White)
                return Player.Color.Black;
            else
                return Player.Color.White;
        }
    }
}
