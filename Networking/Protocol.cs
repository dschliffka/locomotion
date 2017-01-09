using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Locomotion.Networking
{
    public class Protocol
    {
        public enum  Type
        {
            challengeRequest = 1,
            challengeAccepted = 2,
            challengeDenied = 3,
            chatMessage = 4,
            move = 5
        }
    }
}
