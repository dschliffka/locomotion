using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class Disk : GamePiece
    {
        public Disk(Pair pos, Player.Color color)
        {
            this.pos = pos;
            this.color = color;
        }

        public Disk(int row, int col, Player.Color color)
        {
            Pair pair = new Pair(row, col);
            this.pos = pair;
            this.color = color;
        }

        // inherits (bool) isLocated
    }
}
