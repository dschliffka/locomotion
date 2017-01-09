using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion
{
    public class PieceMove
    {
        Pair _to;
        Pair _from;

        public PieceMove(Pair to, Pair from)
        {
            this._to = to;
            this._from = from;
        }

        public Pair to
        {
            get { return this._to; }
            set { this._to = value; }
        }

        public Pair from
        {
            get { return this._from; }
            set { this._from = value; }
        }

        public PieceMove() { }
    }
}
