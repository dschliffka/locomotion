using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class Peg : GamePiece
    {
        public Peg(Pair pos, Player.Color color)
        {
            this.pos = pos;
            this.color = color;
        }

        public Peg(int row, int col, Player.Color color)
        {
            Pair pair = new Pair(row, col);
            this.pos = pair;
            this.color = color;
        }

        // inherits (bool) isLocated
    }
}
