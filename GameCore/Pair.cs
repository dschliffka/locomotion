using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class Pair
    {
        public int row { get; set; }
        public int col { get; set; }

        public Pair( int row, int col )
        {
            this.row = row;
            this.col = col;
        }

        public Pair(){ }
    }
}
