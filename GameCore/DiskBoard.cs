using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class DiskBoard : Board
    {
        /// <summary>
        /// Class member variables
        /// </summary>
        public Disk[,] board;

        private int _whiteDiskCount;
        private int _blackDiskCount;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"></param>
        public DiskBoard(int size)
        {
            this.size = size;
            board = new Disk[size, size];

            // Place corner disks
            board[0, 0] = new Disk(0, 0, Player.Color.White);
            board[0, size - 1] = new Disk(0, size - 1, Player.Color.Black);
            board[size - 1, 0] = new Disk(size - 1, 0, Player.Color.Black);
            board[size - 1, size - 1] = new Disk(size - 1, size - 1, Player.Color.White);
        }

        public DiskBoard(int size, Game.TypeOfGame type, int campaignLevel)
        {
            if (type == Game.TypeOfGame.Campaign && campaignLevel == 2)
            {
                this.size = size;
                board = new Disk[size, size];

                // Place corner disks
                board[0, 0] = new Disk(0, 0, Player.Color.White);
                board[0, size - 1] = new Disk(0, size - 1, Player.Color.Black);
                board[size - 1, 0] = new Disk(size - 1, 0, Player.Color.Black);
                board[size - 1, size - 1] = new Disk(size - 1, size - 1, Player.Color.White);

                board[2, 2] = new Disk(2, 2, Player.Color.Nil);
                board[2, 3] = new Disk(2, 3, Player.Color.Nil);
                board[3, 2] = new Disk(3, 2, Player.Color.Nil);
                board[3, 3] = new Disk(3, 3, Player.Color.Nil);
            }
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="oldDiskBoard"></param>
        public DiskBoard(DiskBoard oldDiskBoard)
        {
            this.size = oldDiskBoard.size;
            this.board = new Disk[size, size];
            Array.Copy(oldDiskBoard.board, this.board, size*size);
            _blackDiskCount = oldDiskBoard._blackDiskCount;
            _whiteDiskCount = oldDiskBoard._whiteDiskCount;
        }

        /// <summary>
        /// Public getters for private members
        /// </summary>
        public int whiteDiskCount
        {
            get { return _whiteDiskCount; }
        }

        public int blackDiskCount
        {
            get { return _blackDiskCount; }
        }

        public void placeDisk(Player mover, Pair to, Pair from)
        {
            placeDisk(mover.color, to, from);
        }

        public void placeDisk(Player.Color color, Pair to, Pair from)
        {
            int row = (from.row + to.row) / 2;
            int col = (from.col + to.col) / 2;

            // place onto empty
            if (this.board[row, col] == null)
            {
                this.board[row, col] = new Disk(row, col, color);
                incDiskCount(color);
            }

            else if ((row == 0 && col == 0) || (row == 5 && col == 0)
                || (row == 0 && col == 5) || (row == 5 && col == 5))
            { }

            // flip over occupied
            else if (this.board[row, col].color != color)
            {
                this.board[row, col] = new Disk(row, col, color);
                adjDiskCount(color);
            }
        }

        private void placeDisk(int row, int col, Player.Color color)
        {
            board[row, col] = new Disk(row, col, color);
        }

        public bool diskExistsAt(Pair pos)
        {
            return diskExistsAt(pos.row, pos.col);
        }
        public bool diskExistsAt(int row, int col)
        {
            return (board[row, col] != null);
        }

        public bool sameColorDiskAt(Pair pos, Player.Color currentColor)
        {
            return sameColorDiskAt(pos.row, pos.col, currentColor);
        }

        public bool sameColorDiskAt(int row, int col, Player.Color currentColor)
        {
            bool sameColor = false;

            if (this.board[row, col] != null)
                if (this.board[row, col].color == currentColor)
                    sameColor = true;
               
            return sameColor;
        }

        private void incDiskCount(Player.Color color)
        {
            // add new disk
            if (color == Player.Color.Black)
                _blackDiskCount++;
            else
                _whiteDiskCount++;
        }

        private void adjDiskCount(Player.Color color)
        {
            // flip opposite disk
            if (color == Player.Color.Black)
            {
                _blackDiskCount++;
                _whiteDiskCount--;
            }
            else
            {
                _whiteDiskCount++;
                _blackDiskCount--;
            }
        }

        public int movesFromWin()
        {
            int count;
            count = Math.Min(movesFromBlackWin(5, 0), movesFromWhiteWin(0, 0));
            return count;
        }

        public int movesFromWin(Player.Color color)
        {
            int count;

            if (color == Player.Color.White)
                count = movesFromWhiteWin(0, 0);
            else
                count = movesFromBlackWin(5, 0);

            return count;
        }

        private short movesFromWhiteWin(int row, int col)
        {
            short count = 0;

            if (row == 5 && col == 5)
                return 0;
            else if (col == 5)
                count = movesFromWhiteWin(row + 1, col);
            else if (row == 5)
                count = movesFromWhiteWin(row, col + 1);
            else
                count = Math.Min(movesFromWhiteWin(row + 1, col), movesFromWhiteWin(row, col + 1));


            if (board[row, col] == null || board[row, col].color != Player.Color.White)
            {
                count++;
            }

            return count;
        }

        private short movesFromBlackWin(int row, int col)
        {
            short count = 0;

            if (row == 0 && col == 5)
                return 0;
            else if (col == 5)
                count = movesFromBlackWin(row - 1, col);
            else if (row == 0)
                count = movesFromBlackWin(row, col + 1);
            else
                count = Math.Min(movesFromBlackWin(row - 1, col), movesFromBlackWin(row, col + 1));


            if (board[row, col] == null || board[row, col].color != Player.Color.Black)
            {
                count++;
            }

            return count;
        }

        public bool checkForWin()
        {
            return checkForWin(Player.Color.White) || checkForWin(Player.Color.Black);
        }

        public bool checkForWin(Player.Color color)
        {
            var moves = new List<Pair>();
            var diskQ = new Queue<Disk>();

            if (color == Player.Color.White)
            {
                diskQ.Enqueue(new Disk(0, 0, Player.Color.White));
                moves.Add(new Pair(0, 0));

                if (whiteDiskCount < 9)
                    return false;
            }
            else
            {
                diskQ.Enqueue(new Disk(0, size-1, Player.Color.Black));
                moves.Add(new Pair(0, size - 1));

                if (blackDiskCount < 9)
                    return false;
            }

            while (diskQ.Count != 0)
            {
                Disk currentDisk = diskQ.Dequeue();
                if (currentDisk.pos.row > 0)
                {
                    // check north
                    if (board[currentDisk.pos.row - 1, currentDisk.pos.col] != null                     // a disk exists
                        && board[currentDisk.pos.row - 1, currentDisk.pos.col].color == color           // matches your color
                        && !listContains(moves, new Pair(currentDisk.pos.row - 1, currentDisk.pos.col)))// hasn't yet been checked
                    {
                        diskQ.Enqueue(new Disk(currentDisk.pos.row - 1, currentDisk.pos.col, currentDisk.color));
                        moves.Add(new Pair(currentDisk.pos.row - 1, currentDisk.pos.col));
                    }
                }
                if (currentDisk.pos.row < (size - 1))
                {
                    // check south
                    if (board[currentDisk.pos.row + 1, currentDisk.pos.col] != null                     // a disk exists
                        && board[currentDisk.pos.row + 1, currentDisk.pos.col].color == color           // matches your color
                        && !listContains(moves, new Pair(currentDisk.pos.row + 1, currentDisk.pos.col)))// hasn't yet been checked
                    {
                        diskQ.Enqueue(new Disk(currentDisk.pos.row + 1, currentDisk.pos.col, currentDisk.color));
                        moves.Add(new Pair(currentDisk.pos.row + 1, currentDisk.pos.col));
                    }
                }
                if (currentDisk.pos.col > 0)
                {
                    // check west
                    if (board[currentDisk.pos.row, currentDisk.pos.col - 1] != null                     // a disk exists
                        && board[currentDisk.pos.row, currentDisk.pos.col - 1].color == color           // matches your color
                        && !listContains(moves, new Pair(currentDisk.pos.row, currentDisk.pos.col - 1)))// hasn't yet been checked
                    {
                        diskQ.Enqueue(new Disk(currentDisk.pos.row, currentDisk.pos.col - 1, currentDisk.color));
                        moves.Add(new Pair(currentDisk.pos.row, currentDisk.pos.col - 1));
                    }
                }
                if (currentDisk.pos.col < (size - 1))
                {
                    // check east
                    if (board[currentDisk.pos.row, currentDisk.pos.col + 1] != null                     // a disk exists
                        && board[currentDisk.pos.row, currentDisk.pos.col + 1].color == color           // matches your color
                        && !listContains(moves, new Pair(currentDisk.pos.row, currentDisk.pos.col + 1)))// hasn't yet been checked
                    {
                        diskQ.Enqueue(new Disk(currentDisk.pos.row, currentDisk.pos.col + 1, currentDisk.color));
                        moves.Add(new Pair(currentDisk.pos.row, currentDisk.pos.col + 1));
                    }
                }


                // Check if we have won
                if (color == Player.Color.White 
                    && currentDisk.isLocated(size-1,size-1))
                {
                    return true;
                }
                else if (color == Player.Color.Black
                    && currentDisk.isLocated(size - 1, 0))
                {
                    return true;
                }
            }

            // No more moves available
            return false;
        }

        private bool listContains(List<Pair> moves, Pair pos)
        {
            // support function for checkForWin
            foreach (Pair pair in moves)
            {
                if (pair.row == pos.row && pair.col == pos.col)
                    return true;
            }

            // if not found
            return false;
        }

        public List<TrainTree> getWinningPath(Player.Color color)
        {
            int[,] disks = new int[6, 6];
            // bread crumbs are laid on disks mark up board
            for( int r = 0; r < 6; r++)
                for (int c = 0; c < 6; c++)
                {
                    disks[r,c] = 0;
                    if (r < 5 && board[r + 1, c] != null && board[r + 1, c].color == color) // S
                        disks[r, c] += 15;
                    if (r > 0 && board[r - 1, c] != null && board[r - 1, c].color == color) // N
                        disks[r, c] += 1;
                    if (c < 5 && board[r, c + 1] != null && board[r, c + 1].color == color) // E
                        disks[r, c] += 7;
                    if (c > 0 && board[r, c - 1] != null && board[r, c - 1].color == color) // W
                        disks[r, c] += 3;
                }
            

            List<TrainTree> winningPath = new List<TrainTree>();
            Pair start, end;
            if (color == Player.Color.White)
            {
                start = new Pair(0, 0);
                end = new Pair(5, 5);
            }
            else
            {
                start = new Pair(0, 5);
                end = new Pair(5, 0);
            }


            TrainTree root = new TrainTree(start.row, start.col, null);
            disks[start.row, start.col] = 0;
            TrainTree leaf = null;
            Queue<TrainTree> findPath = new Queue<TrainTree>();
            TrainTree current;
            current = root;
            findPath.Enqueue(root);


            while(leaf == null)
            {
                current = findPath.Dequeue();
                if (current.Row < 5
                    && board[current.Row + 1, current.Col] != null
                    && board[current.Row + 1, current.Col].color == color // S
                    && disks[current.Row + 1, current.Col] > 0)
                {
                    findPath.Enqueue(new TrainTree(current.Row + 1, current.Col, current));
                    disks[current.Row + 1, current.Col] = 0;
                }

                if (current.Row > 0
                    && board[current.Row - 1, current.Col] != null
                    && board[current.Row - 1, current.Col].color == color // N
                    && disks[current.Row - 1, current.Col] > 0)
                {
                    findPath.Enqueue(new TrainTree(current.Row - 1, current.Col, current));
                    disks[current.Row - 1, current.Col] = 0;
                }

                if (current.Col < 5
                    && board[current.Row, current.Col + 1] != null
                    && board[current.Row, current.Col + 1].color == color // E
                    && disks[current.Row, current.Col + 1] > 0)
                {
                    findPath.Enqueue(new TrainTree(current.Row, current.Col + 1, current));
                    disks[current.Row, current.Col + 1] = 0;
                }

                if (current.Col > 0
                    && board[current.Row, current.Col - 1] != null
                    && board[current.Row, current.Col - 1].color == color // W
                    && disks[current.Row, current.Col - 1] > 0)
                {
                    findPath.Enqueue(new TrainTree(current.Row, current.Col - 1, current));
                    disks[current.Row, current.Col - 1] = 0;
                }


                if (current.Row == end.row && current.Col == end.col)
                    leaf = current;
            }

            // identify singular rout
            current = leaf;
            while( current != null)
            {
                winningPath.Add(current);
                current = current.Previous;
            }

            // reinitialize disks markup board
            for( int r = 0; r < 6; r++)
                for (int c = 0; c < 6; c++)
                {
                    disks[r, c] = 0;
                }

            // mark winning disks
            foreach (TrainTree tt in winningPath)
            {
                disks[tt.Row, tt.Col] = 1;
            }

            // identify the track index;
            foreach (TrainTree tt in winningPath)
            {
                tt.DirectionalIndex = 0;
                if (tt.Row < 5 && disks[tt.Row + 1, tt.Col] != 0) // S
                    tt.DirectionalIndex += 15;
                if (tt.Row > 0 && disks[tt.Row - 1, tt.Col] != 0) // N
                    tt.DirectionalIndex += 1;
                if (tt.Col < 5 && disks[tt.Row, tt.Col + 1] != 0) // E
                    tt.DirectionalIndex += 7;
                if (tt.Col > 0 && disks[tt.Row, tt.Col - 1] != 0) // W
                    tt.DirectionalIndex += 3;
            }

            return winningPath;
        }

        public void initializeTutorialForWin()
        {
            board = new Disk[size, size];

            placeDisk(0, 0, Player.Color.White);
            placeDisk(0, 1, Player.Color.White);
            placeDisk(1, 0, Player.Color.White);
            placeDisk(1, 1, Player.Color.White);
            placeDisk(2, 1, Player.Color.White);
            placeDisk(3, 1, Player.Color.White);
            placeDisk(3, 2, Player.Color.White);
            placeDisk(3, 3, Player.Color.White);
            placeDisk(4, 3, Player.Color.White);
            placeDisk(4, 5, Player.Color.White);
            placeDisk(5, 3, Player.Color.White);
            placeDisk(5, 4, Player.Color.White);
            placeDisk(5, 5, Player.Color.White);
            placeDisk(0, 3, Player.Color.Black);
            placeDisk(0, 4, Player.Color.Black);
            placeDisk(1, 3, Player.Color.Black);
            placeDisk(1, 4, Player.Color.Black);
            placeDisk(1, 5, Player.Color.Black);
            placeDisk(2, 2, Player.Color.Black);
            placeDisk(2, 3, Player.Color.Black);
            placeDisk(4, 0, Player.Color.Black);
            placeDisk(4, 1, Player.Color.Black);
            placeDisk(4, 2, Player.Color.Black);
        }
    }
}
