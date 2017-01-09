using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Locomotion
{
    public class PegBoard : Board
    {
        /// <summary>
        /// Class member variables
        /// </summary>
        public List<Peg> pegList;

        private int _whitePegCount;
        private int _blackPegCount;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"></param>
        public PegBoard(int size)
        {
            this.size = size;
            pegList = new List<Peg>();

            // 'Invalid' corner pegs
            pegList.Add( new Peg(0,0, Player.Color.Nil) );
            pegList.Add( new Peg(0, size-1, Player.Color.Nil) );
            pegList.Add( new Peg(size-1, 0, Player.Color.Nil) );
            pegList.Add( new Peg(size-1, size-1, Player.Color.Nil) );

            placePegs();
        }

        public PegBoard(int size, Game.TypeOfGame type, int campaignLevel)
        {
            this.size = size;
            pegList = new List<Peg>();

            // 'Invalid' corner pegs
            pegList.Add(new Peg(0, 0, Player.Color.Nil));
            pegList.Add(new Peg(0, size - 1, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, 0, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, size - 1, Player.Color.Nil));

            pegList.Add(new Peg(3, 3, Player.Color.Nil));

            placePegs();
        }

        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="oldPegBoard"></param>
        public PegBoard (PegBoard oldPegBoard)
        {
            this.pegList = new List<Peg>(oldPegBoard.pegList);
            this._whitePegCount = oldPegBoard._whitePegCount;
            this._blackPegCount = oldPegBoard._blackPegCount;
        }

        /// <summary>
        /// Public getters for private members
        /// </summary>
        public int whitePegCount
        {
            get { return this._whitePegCount; }
        }

        public int blackPegCount
        {
            get { return this._blackPegCount; }
        }

        public void placePegs()
        {
            // White Pegs
            pegList.Add(new Peg(0, 1, Player.Color.White));
            pegList.Add(new Peg(0, 2, Player.Color.White));
            pegList.Add(new Peg(1, 0, Player.Color.White));
            pegList.Add(new Peg(2, 0, Player.Color.White));
            pegList.Add(new Peg(size - 1, size - 2, Player.Color.White));
            pegList.Add(new Peg(size - 1, size - 3, Player.Color.White));
            pegList.Add(new Peg(size - 2, size - 1, Player.Color.White));
            pegList.Add(new Peg(size - 3, size - 1, Player.Color.White));

            // Black Pegs
            pegList.Add( new Peg(size - 2, 0, Player.Color.Black));
            pegList.Add( new Peg(size - 3, 0, Player.Color.Black));
            pegList.Add( new Peg(size - 1, 1, Player.Color.Black));
            pegList.Add( new Peg(size - 1, 2, Player.Color.Black));
            pegList.Add( new Peg(0, size - 2, Player.Color.Black));
            pegList.Add( new Peg(0, size - 3, Player.Color.Black));
            pegList.Add( new Peg(1, size - 1, Player.Color.Black));
            pegList.Add( new Peg(2, size - 1, Player.Color.Black));

            // Initialize peg counts
            _whitePegCount = 8;
            _blackPegCount = 8;
        }

        public void movePeg(Player.Color color, Pair to, Pair from)
        {
            if (Math.Abs(to.row - from.row) == 2
                ^ Math.Abs(to.col - from.col) == 2)
            {
                // Capture then move
                capturePeg(color, to, from);
            }

            foreach (Peg peg in this.pegList)
            {
                if (peg.isLocated(from))
                {
                    this.pegList.Remove(peg);
                    this.pegList.Add(new Peg(to, peg.color));
                    return;
                }
            }
        }

        public void capturePeg(Player.Color color, Pair to, Pair from)
        {
            // Find the peg
            foreach (Peg peg in this.pegList) 
            {
                // Remove jumped peg
                if (peg.isLocated((from.row + to.row) / 2, (from.col + to.col) / 2))
                {
                    this.pegList.Remove(peg);

                    if (color == Player.Color.White)
                        this._blackPegCount--;
                    else
                        this._whitePegCount--;
                    
                    return;
                }
            }
        }

        public bool isEmpty( Pair spot) 
        {
            return isEmpty(spot.row, spot.col);
        }

        public bool isEmpty(int row, int col)
        {
            // returns true if the requested peg slot is empty
            foreach (Peg peg in pegList)
            {
                if( peg.isLocated(row, col) )
                {
                    return false;
                }
            }

            return true;
        }

        public Player.Color checkColor(Pair spot)
        {
            return checkColor(spot.row, spot.col);
        }

        public Player.Color checkColor(int row, int col)
        {
            foreach (Peg peg in pegList)
            {
                if( peg.isLocated(row, col) )
                    return peg.color;
            }
            return Player.Color.Nil;
        }

        public bool checkForTie()
        {
            // returns true for a tie
            return (_whitePegCount == 0 || _blackPegCount == 0);
        }

        public int myPegCount(Player.Color color)
        {
            if (color == Player.Color.White)
                return _whitePegCount;
            else
                return _blackPegCount;
        }

        public void initializeTutorial()
        {
            pegList.Clear();
            pegList.Add(new Peg(0, 0, Player.Color.Nil));
            pegList.Add(new Peg(0, size - 1, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, 0, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, size - 1, Player.Color.Nil));
            pegList.Add(new Peg(3, 3, Player.Color.White));
        }

        public void initializeTutorialWithEnemy()
        {
            pegList.Clear();
            pegList.Add(new Peg(0, 0, Player.Color.Nil));
            pegList.Add(new Peg(0, size - 1, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, 0, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, size - 1, Player.Color.Nil));
            pegList.Add(new Peg(3, 3, Player.Color.White));
            pegList.Add(new Peg(3, 4, Player.Color.Black));
        }

        public void initializeTutorialForWin()
        {
            pegList.Clear();
            pegList.Add(new Peg(0, 0, Player.Color.Nil));
            pegList.Add(new Peg(0, size - 1, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, 0, Player.Color.Nil));
            pegList.Add(new Peg(size - 1, size - 1, Player.Color.Nil));

            pegList.Add(new Peg(1, 1, Player.Color.White));
            pegList.Add(new Peg(3, 2, Player.Color.White));
            pegList.Add(new Peg(3, 3, Player.Color.White));
            pegList.Add(new Peg(4, 5, Player.Color.White));
            pegList.Add(new Peg(5, 5, Player.Color.White));
            pegList.Add(new Peg(6, 4, Player.Color.White));

            pegList.Add(new Peg(0, 4, Player.Color.Black));
            pegList.Add(new Peg(1, 3, Player.Color.Black));
            pegList.Add(new Peg(2, 4, Player.Color.Black));
            pegList.Add(new Peg(3, 5, Player.Color.Black));
            pegList.Add(new Peg(4, 1, Player.Color.Black));
            pegList.Add(new Peg(4, 3, Player.Color.Black));
        }
    }
}
