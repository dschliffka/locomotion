using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion.Networking
{
    public class ConnectionEventArgs: System.EventArgs
    {
        public bool connected;
        public string message;
    }
}
