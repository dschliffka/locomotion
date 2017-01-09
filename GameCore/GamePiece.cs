using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public abstract class GamePiece
    {
        /// <summary>
        /// Class member variables
        /// </summary>
        protected Pair _pos;
        protected Player.Color _color;


        /// <summary>
        /// Public getters and protected setters
        /// </summary>
        public Pair pos
        {
            get { return _pos; }
            protected set { this._pos = value; }
        }

        public Player.Color color
        {
            get { return _color; }
            protected set { this._color = value; }
        }


        /// <summary>
        /// Class method: (bool) if GamePiece is located at the parameter
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public bool isLocated(int row, int col)
        {
            // returns true for a matching location
            return (row == this.pos.row
                && col == this.pos.col);
        }

        public bool isLocated(Pair candidate)
        {
            return isLocated(candidate.row, candidate.col);
        }
    }
}
