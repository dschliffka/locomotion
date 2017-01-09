using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Locomotion
{
    public class TreeNode
    {
        private int _maxTreeDepth;
        private int _movesFromWin;
        public enum MiniMax { Min, Max };
        public enum Difficulty { Easy, Medium, Hard };
        public enum MoveType { Lateral, Jump, Disk };
        private static List<Pair> _moveSet= GenerateMoveSet();

        // for this generation:
        private int _heuristicValue;
        private int _levelInTree;
        private List<TreeNode> _children;
        private PieceMove _moveMade;
        private MoveType _moveType;
        private double _middlePegHeuristic = 0;

        // for next generation:
        PegBoard _pegBoard;
        MiniMax _minimaxValue;
        Player.Color _currentColor;
        Player.Color _aiColor;
        DiskBoard _diskBoard;
        Difficulty _difficultyLevel;


        /// <summary>
        /// Public Constructor
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pegBoard"></param>
        /// <param name="diskBoard"></param>
        /// <param name="minimaxValue"></param>
        public TreeNode(Player player, PegBoard pegBoard, DiskBoard diskBoard, MiniMax minimaxValue, int treeDepth)
        {
            this._minimaxValue = minimaxValue;
            this._pegBoard = pegBoard;
            this._currentColor = this._aiColor = player.color;
            this._diskBoard = diskBoard;
            this._movesFromWin = diskBoard.movesFromWin();
            this._maxTreeDepth = treeDepth;

            if (treeDepth == 1)
                this._difficultyLevel = Difficulty.Easy;
            else if (treeDepth == 3)
                this._difficultyLevel = Difficulty.Medium;
            else
                this._difficultyLevel = Difficulty.Hard;
        }
        
        /// <summary>
        /// Private Constructor used to initialize child node
        /// </summary>
        /// <param name="currentColor"></param> ---> will switch color
        /// <param name="pegBoard"></param>
        /// <param name="diskBoard"></param>
        /// <param name="minimaxValue"></param> ---> will switch min/max
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="levelInTree"></param>
        /// <param name="aiColor"></param>
        private TreeNode(Player.Color currentColor, PegBoard pegBoard, DiskBoard diskBoard, MiniMax minimaxValue, Pair to, Pair from, int levelInTree, Player.Color aiColor, int movesFromWin, int maxTreeDepth, double middlePegHeuristic)
        {
            this._pegBoard = pegBoard;
            this._diskBoard = diskBoard;
            this._moveMade = new PieceMove();
            this._moveMade.to = to;
            this._moveMade.from = from;
            this._levelInTree = levelInTree;
            this._aiColor = aiColor;
            this._movesFromWin = movesFromWin - 1;
            this._maxTreeDepth = maxTreeDepth;
            this._middlePegHeuristic = middlePegHeuristic;

            if (maxTreeDepth == 1)
                this._difficultyLevel = Difficulty.Easy;
            else if (maxTreeDepth == 3)
                this._difficultyLevel = Difficulty.Medium;
            else
                this._difficultyLevel = Difficulty.Hard;

            // Switch colors
            if (currentColor == Player.Color.White)
                this._currentColor = Player.Color.Black;
            else
                this._currentColor = Player.Color.White;

            // Switch minimaxValue
            if (minimaxValue == MiniMax.Max)
                this._minimaxValue = MiniMax.Min;
            else
                this._minimaxValue = MiniMax.Max;
        }

        /// <summary>
        /// Public getters for private members
        /// </summary>
        public List<TreeNode> children
        {
            get { return this._children; }
        }

        public int heuristicValue
        {
            get { return this._heuristicValue; }
        }

        public PieceMove moveMade
        {
            get { return this._moveMade; }
        }
        #region HEURISTICS

        /// <summary>
        /// Function to evaluate the node for a difficulty level of Hard
        /// </summary>
        /// <param name="aiColor"></param>
        /// <returns>returns an integer representing the heuristic value</returns>
        private int HardHeuristic(Player.Color aiColor)
        {
            int heuristicValue = 0;

            if (aiColor == Player.Color.Black)
            {
                if (_pegBoard.whitePegCount == 0)
                {
                    heuristicValue = 10000;
                    this._heuristicValue = heuristicValue;
                    return heuristicValue;
                }

                int numBlackFromWin = _diskBoard.movesFromWin(Player.Color.Black);
                int numWhiteFromWin = _diskBoard.movesFromWin(Player.Color.White);
                int blackPegModifier = 100;
                int blackWinMultiplier = 45;

                //if the last move blocked white from winning, take this move
                if (this._levelInTree == 1 && numWhiteFromWin == 2 && numBlackFromWin != 0)
                {
                    heuristicValue = -2000;
                }
                else if (this._levelInTree == 1 && numBlackFromWin == 0) //if the last move wins for black, take this move
                {
                    heuristicValue = -2000;
                    return heuristicValue;
                }

                if (numWhiteFromWin == 0)
                {
                    heuristicValue = 2000;
                }
                else if (numBlackFromWin == 0)
                {
                    heuristicValue = -2000;
                    heuristicValue += _levelInTree * 70;
                }
                else
                {

                    heuristicValue += (int)(7 * this._middlePegHeuristic);

                    if (this._pegBoard.whitePegCount <= 2)
                    {
                        blackWinMultiplier = 80;
                    }

                    heuristicValue += 120 * (this._pegBoard.whitePegCount - this._pegBoard.blackPegCount);

                    heuristicValue -= blackPegModifier * this._pegBoard.blackPegCount;
                    heuristicValue += 50 * this._pegBoard.whitePegCount;

                    heuristicValue -= 20 * _diskBoard.blackDiskCount;
                    heuristicValue += 25 * _diskBoard.whiteDiskCount;

                    heuristicValue += blackWinMultiplier * numBlackFromWin;
                    heuristicValue -= 40 * numWhiteFromWin;

                }
            }
            else // aiColor == white
            {
                /*if (_pegBoard.blackPegCount == 0 || _pegBoard.whitePegCount == 0)
                {
                    heuristicValue = 10000;
                    this._heuristicValue = heuristicValue;
                    return heuristicValue;
                }

                int numBlackFromWin = _diskBoard.movesFromWin(Player.Color.Black);
                int numWhiteFromWin = _diskBoard.movesFromWin(Player.Color.White);

                if (numBlackFromWin == 0)
                    heuristicValue = 1000;
                else if (numWhiteFromWin == 0)
                {
                    heuristicValue = -2000;
                    heuristicValue += _levelInTree * 70;
                }
                else
                {
                    heuristicValue -= 50 * this._pegBoard.whitePegCount;
                    heuristicValue += 50 * this._pegBoard.blackPegCount;

                    heuristicValue -= 7 * _diskBoard.whiteDiskCount;
                    heuristicValue += 7 * _diskBoard.blackDiskCount;

                    heuristicValue += 15 * numWhiteFromWin;
                    heuristicValue -= 15 * numBlackFromWin;
                }

            }*/


                if (_pegBoard.blackPegCount == 0)
                {
                    heuristicValue = 10000;
                    this._heuristicValue = heuristicValue;
                    return heuristicValue;
                }

                int numBlackFromWin = _diskBoard.movesFromWin(Player.Color.Black);
                int numWhiteFromWin = _diskBoard.movesFromWin(Player.Color.White);
                int whitePegModifier = 100;
                int whiteWinMultiplier = 45;

                //if the last move blocked white from winning, take this move
                if (this._levelInTree == 1 && numBlackFromWin == 2 && numWhiteFromWin != 0)
                {
                    heuristicValue = -2000;
                }
                else if (this._levelInTree == 1 && numWhiteFromWin == 0) //if the last move wins for white, take this move
                {
                    heuristicValue = -2000;
                    return heuristicValue;
                }

                if (numBlackFromWin == 0)
                {
                    heuristicValue = 2000;
                }
                else if (numWhiteFromWin == 0)
                {
                    heuristicValue = -2000;
                    heuristicValue += _levelInTree * 70;
                }
                else
                {

                    heuristicValue += (int)(7 * this._middlePegHeuristic);

                    if (this._pegBoard.blackPegCount <= 2)
                    {
                        whiteWinMultiplier = 80;
                    }

                    heuristicValue += 120 * (this._pegBoard.blackPegCount - this._pegBoard.whitePegCount);

                    heuristicValue += 50 * this._pegBoard.blackPegCount;
                    heuristicValue -= whitePegModifier * this._pegBoard.whitePegCount;

                    heuristicValue += 25 * _diskBoard.blackDiskCount;
                    heuristicValue -= 20 * _diskBoard.whiteDiskCount;

                    heuristicValue -= 40 * numBlackFromWin;
                    heuristicValue += whiteWinMultiplier * numWhiteFromWin;

                }
            }

            return heuristicValue;
        }


        /// <summary>
        /// Function to evaluate the node for a difficulty level of Medium
        /// </summary>
        /// <param name="aiColor"></param>
        /// <returns>returns an integer representing the heuristic value</returns>
        private int MediumHeuristic(Player.Color aiColor)
        {
            int heuristicValue = 0;

            if (aiColor == Player.Color.Black)
            {
                if (_pegBoard.blackPegCount == 0 || _pegBoard.whitePegCount == 0)
                {
                    heuristicValue = 10000;
                    this._heuristicValue = heuristicValue;
                    return heuristicValue;
                }

                int numBlackFromWin = _diskBoard.movesFromWin(Player.Color.Black);
                int numWhiteFromWin = _diskBoard.movesFromWin(Player.Color.White);

                if (numWhiteFromWin == 0)
                    heuristicValue = 1000;
                else if (numBlackFromWin == 0)
                {
                    heuristicValue = -1000;
                    heuristicValue += _levelInTree * 70;
                }
                else
                {
                    heuristicValue -= 25 * this._pegBoard.blackPegCount;
                    heuristicValue += 25 * this._pegBoard.whitePegCount;

                    heuristicValue -= 10 * _diskBoard.blackDiskCount;
                    heuristicValue += 10 * _diskBoard.whiteDiskCount;

                    heuristicValue += 15 * numBlackFromWin;
                    heuristicValue -= 15 * numWhiteFromWin;

                }
            }
            else
            {
                if (_pegBoard.blackPegCount == 0 || _pegBoard.whitePegCount == 0)
                {
                    heuristicValue = 10000;
                    this._heuristicValue = heuristicValue;
                    return heuristicValue;
                }

                int numBlackFromWin = _diskBoard.movesFromWin(Player.Color.Black);
                int numWhiteFromWin = _diskBoard.movesFromWin(Player.Color.White);

                if (numBlackFromWin == 0)
                    heuristicValue = 1000;
                else if (numWhiteFromWin == 0)
                {
                    heuristicValue = -1000;
                    heuristicValue += _levelInTree * 70;
                }
                else
                {
                    heuristicValue -= 25 * this._pegBoard.whitePegCount;
                    heuristicValue += 25 * this._pegBoard.blackPegCount;

                    heuristicValue -= 10 * _diskBoard.whiteDiskCount;
                    heuristicValue += 10 * _diskBoard.blackDiskCount;

                    heuristicValue += 15 * numWhiteFromWin;
                    heuristicValue -= 15 * numBlackFromWin;
                }
            }

            return heuristicValue;
        }


        /// <summary>
        /// Function to evaluate the node for a difficulty level of Easy
        /// </summary>
        /// <param name="aiColor"></param>
        /// <returns>returns an integer representing the heuristic value</returns>
        private int EasyHeuristic(Player.Color aiColor)
        {
            int heuristicValue = 0;

            if (aiColor == Player.Color.Black)
            {
                if (_pegBoard.blackPegCount == 0 || _pegBoard.whitePegCount == 0)
                {
                    heuristicValue = 10000;
                    this._heuristicValue = heuristicValue;
                    return heuristicValue;
                }

                int numBlackFromWin = _diskBoard.movesFromWin(Player.Color.Black);
                int numWhiteFromWin = _diskBoard.movesFromWin(Player.Color.White);

                if (numWhiteFromWin == 0)
                    heuristicValue = 1000;
                else if (numBlackFromWin == 0)
                {
                    heuristicValue = -1000;
                    heuristicValue += _levelInTree * 70;
                }
                else
                {
                    //make AI randomly make a lateral move not taking a disk or peg.
                    Random random = new Random();

                    int dumbOrNot = random.Next(-1, 3);

                    random = null;

                    heuristicValue -= 0 * this._pegBoard.blackPegCount;
                    heuristicValue += 2 * this._pegBoard.whitePegCount;

                    heuristicValue -= dumbOrNot * 8 * _diskBoard.blackDiskCount;
                    heuristicValue += 0 * _diskBoard.whiteDiskCount;

                    heuristicValue += 0 * numBlackFromWin;
                    heuristicValue -= 0 * numWhiteFromWin;
                }
            }
            else
            {
                if (_pegBoard.blackPegCount == 0 || _pegBoard.whitePegCount == 0)
                {
                    heuristicValue = 10000;
                    this._heuristicValue = heuristicValue;
                    return heuristicValue;
                }

                int numBlackFromWin = _diskBoard.movesFromWin(Player.Color.Black);
                int numWhiteFromWin = _diskBoard.movesFromWin(Player.Color.White);

                if (numBlackFromWin == 0)
                    heuristicValue = 1000;
                else if (numWhiteFromWin == 0)
                {
                    heuristicValue = -1000;
                    heuristicValue += _levelInTree * 70;
                }
                else
                {
                    heuristicValue -= 10 * this._pegBoard.whitePegCount;
                    heuristicValue += 10 * this._pegBoard.blackPegCount;

                    heuristicValue -= 8 * _diskBoard.whiteDiskCount;
                    heuristicValue += 8 * _diskBoard.blackDiskCount;

                    heuristicValue += 4 * numWhiteFromWin;
                    heuristicValue -= 4 * numBlackFromWin;
                }
            }

            return heuristicValue;
        }

        private int ApplyHeuristic(Player.Color aiColor)
        {
            int heuristicValue = 0;

            if (this._difficultyLevel == Difficulty.Easy)
            {
                heuristicValue = EasyHeuristic(aiColor);
            }
            else if (this._difficultyLevel == Difficulty.Medium)
            {
                heuristicValue = MediumHeuristic(aiColor);
            }
            else
            {
                heuristicValue = HardHeuristic(aiColor);
            }

            this._heuristicValue = heuristicValue;

            return heuristicValue;
        }
        #endregion

        public int AlphaBeta(TreeNode root, int alpha, int beta)
        {
            if (root._levelInTree >= root._maxTreeDepth)
            {
               return root.ApplyHeuristic(root._aiColor);
            }
            else if (root._levelInTree == 0)
            {
                root._children = PopulateTree(root);
                int max = root._children.Count;

                ParallelOptions op = new ParallelOptions();
                op.MaxDegreeOfParallelism = 40;
                Parallel.For(0, max, op, i =>
                {
                    root.children.ElementAt(i)._heuristicValue = AlphaBeta(root.children.ElementAt(i), alpha, beta);
                });

                return Int32.MaxValue;
            }
            else
            {
                bool win = root._diskBoard.checkForWin(this._aiColor);

                //if the last move was a winning move, evaluate this node and don't dig deeper!
                if ((_movesFromWin <= 1 && root._diskBoard.movesFromWin(root.oppositeColor(root._aiColor)) == 0 && root._levelInTree == 2)
                    || (_movesFromWin <= 1 && root._levelInTree == 1 && win)
                    || root._pegBoard.blackPegCount==0 || root._pegBoard.whitePegCount==0)
                {
                    return root.ApplyHeuristic(_aiColor);
                }

                if (root._levelInTree == 1)
                {

                    foreach (Peg peg in root._pegBoard.pegList)
                    {
                        if (peg.color == this._aiColor)
                        {
                            if (peg.pos.row >= peg.pos.col)
                            {
                                if (peg.pos.col == 0)
                                    root._middlePegHeuristic += peg.pos.row;
                                else
                                    root._middlePegHeuristic += (peg.pos.row / peg.pos.col);
                            }
                            else
                            {
                                if (peg.pos.row == 0)
                                    root._middlePegHeuristic += peg.pos.col;
                                else
                                    root._middlePegHeuristic += (peg.pos.col / peg.pos.row);
                            }
                        }
                    }
                }
                root._children = PopulateTree(root);

                int k = 0;

                if (root._minimaxValue == MiniMax.Max)
                {
                    while (k < root._children.Count)
                    {
                        alpha = Math.Max(alpha, (AlphaBeta(root._children.ElementAt(k), alpha, beta)));

                        if (alpha >= beta)
                        {
                            root._heuristicValue = beta;
                            root._children = null;
                            return beta;
                        }

                        k++;
                    }

                    root._heuristicValue = alpha;
                    return alpha;
                }
                else
                {
                    while (k < root._children.Count)
                    {
                        beta = Math.Min(beta, AlphaBeta(root._children.ElementAt(k), alpha, beta));

                        if (beta <= alpha)
                        {
                            root._heuristicValue = alpha;
                            root._children = null;
                            return alpha;
                        }

                        k++;
                    }
                    root._heuristicValue = beta;
                    return beta;
                }
            }
        }

        /// <summary>
        /// Populates each child node pushing every possible move
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private List<TreeNode> PopulateTree( TreeNode parent)
        {
            List<TreeNode> children = new List<TreeNode>();

            //loop through each peg
            //foreach (Peg peg in parent._pegBoard.pegList)
            ParallelOptions op = new ParallelOptions();
            op.MaxDegreeOfParallelism = 40;

            Parallel.ForEach(parent._pegBoard.pegList, op, peg =>
            {
                //if the peg is the same color as this node
                if (peg.color == parent._currentColor)
                {
                    //loop through all possible moves
                    foreach (Pair pair in _moveSet)
                    {

                        //if the move keeps us on the board, and it is a valid move
                        if (peg.pos.row + pair.row >= 0
                            && peg.pos.row + pair.row < _pegBoard.size
                            && peg.pos.col + pair.col >= 0
                            && peg.pos.col + pair.col < _pegBoard.size
                            && isValid(new Pair(peg.pos.row + pair.row, peg.pos.col + pair.col), peg.pos, parent._pegBoard, parent._currentColor))
                        {
                            //create a new pegBoard and make the move on the new board
                            PegBoard pegBoard = new PegBoard(parent._pegBoard);
                            DiskBoard diskBoard = new DiskBoard(parent._diskBoard);

                            PieceMove pm = new PieceMove(new Pair(peg.pos.row + pair.row, peg.pos.col + pair.col), peg.pos);

                            pegBoard.movePeg(parent._currentColor, pm.to, pm.from);
                            if ((Math.Abs(pair.row) == 1
                                && Math.Abs(pair.col) == 1))
                            {
                                diskBoard.placeDisk(parent._currentColor, pm.to, pm.from);
                            }
                          

                            // change the diskBoard

                            //add this new node as a child node
                            lock (this)
                            {
                                children.Add(new TreeNode(parent._currentColor, pegBoard, diskBoard, parent._minimaxValue, new Pair(peg.pos.row + pair.row, peg.pos.col + pair.col), peg.pos, parent._levelInTree + 1, parent._aiColor, parent._movesFromWin, parent._maxTreeDepth, parent._middlePegHeuristic));
                            }
                        }
                    }
                }
            });

            return children;
        }

        //Generate all possible moves a peg could make
        private static List<Pair> GenerateMoveSet()
        {
            List<Pair> moveSet = new List<Pair>();
            moveSet.Add(new Pair(1, -1)); // SW
            moveSet.Add(new Pair(-1, 1)); // NE
            moveSet.Add(new Pair(-1, -1)); // NW
            moveSet.Add(new Pair(1, 1)); // SE
            moveSet.Add(new Pair(2, 0)); // Jump S
            moveSet.Add(new Pair(0, 2)); // Jump E
            moveSet.Add(new Pair(-2, 0)); // Jump N
            moveSet.Add(new Pair(0, -2)); // Jump W

            moveSet.Add(new Pair(1, 0)); // South
            moveSet.Add(new Pair(0, 1)); // East
            moveSet.Add(new Pair(-1, 0)); // North
            moveSet.Add(new Pair(0, -1)); // West

            return moveSet;
        }

        public bool isValid(Pair to, Pair from, PegBoard pegBoard, Player.Color currentPlayerColor)
        {
            int moveUpDown = from.row - to.row;
            int moveLeftRight = from.col - to.col;

            if (pegBoard.checkColor(from) != currentPlayerColor // correct color
                || pegBoard.isEmpty(from)     // selected empty peg slot
                || !pegBoard.isEmpty(to))  // dropped on occupied slot
            {
                // conditions for immediate failure
                return false;
            }

            else if (Math.Abs(moveUpDown) == 1
                || Math.Abs(moveLeftRight) == 1)
            {
                // jump OR move 1
                return true;
            }

            else if (Math.Abs(moveUpDown) == 2
                ^ Math.Abs(moveLeftRight) == 2) // xor jump (2)
            {
                // Jumps A color and Jumps OTHER color
                return (!pegBoard.isEmpty(from.row - moveUpDown / 2, from.col - moveLeftRight / 2)
                        && pegBoard.checkColor(from.row - moveUpDown / 2, from.col - moveLeftRight / 2) != currentPlayerColor);
            }

            else
            {
                // Don't jump the rainbow!
                return false;
            }
        }

        private Player.Color oppositeColor(Player.Color color)
        {
            if (color == Player.Color.Black)
                return Player.Color.White;
            else
                return Player.Color.Black;
        }
    }
}
