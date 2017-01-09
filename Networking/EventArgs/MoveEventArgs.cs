using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion.Networking
{
    public class MoveEventArgs: System.EventArgs
    {
        public int fromRow;
        public int fromCol;
        public int toRow;
        public int toCol;
        public string playerName;

    }
}
