using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Lidgren.Network;
using System.Threading;
using Locomotion.Networking;
using System.Windows;

namespace Locomotion
{
   //Delegates Here
    public delegate void MessageEventHandler(object sender, MessageEventArgs message);
    public delegate void MoveEventHandler(object sender, MoveEventArgs move);
    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs status);
    public delegate void ChallengeEventHandler(object sender, ChallengeEventArgs challenge);
    public delegate void OpponentEventHandler(object sender, OpponentEventArgs opponent);

    public sealed class NetworkManager
    {
        public bool allowKillNetworkThread = true;

        private static NetworkManager instance;

        private int PORT;

        private string SIGNATURE;

        static private LocoPeer locoPeer;

        private List<NetIncomingMessage> peersList;

        public NetIncomingMessage challenger;

        public string challengername;

        //private static object lockingObject = new object();

        public static NetworkManager InstanceCreator()
        {

            if (instance == null)
            {
                //lock (lockingObject)
                //{
                //    if (instance == null)
                //    {
                        instance = new NetworkManager();
                //    }
                //}
            }
            return instance;
        }

        public LocoPeer getLocoPeer()
        {
            return locoPeer;
        }

        private NetworkManager()
        {
            peersList = new List<NetIncomingMessage>();

            SIGNATURE = "creeper";
            PORT = 14242;

            challenger = null;
            challengername = "";
        }

        #region "Peer"

        //Should receive 4 ints, 1 string for the name
        public void sendPeerMove(string playerName, int fromRow, int fromCol, int toRow, int toCol)
        {
            locoPeer.sendMove(playerName, fromRow, fromCol, toRow, toCol);
        }

        public void sendPeerMessage(string om)
        {
            locoPeer.sendMessage(Protocol.Type.chatMessage, om, locoPeer.NetWorker.challenger);
        }
       
        public string createPeer(string peerName)
        {
            //string newname = addRandomNumber(peerName);

            string newname = peerName;
            if (locoPeer != null)
            {
                string reason = "";
                this.getLocoPeer().Disconnect(reason);
            }
            locoPeer = new LocoPeer(SIGNATURE, newname, PORT);            
            return newname;
        }

        //public string addRandomNumber(string peerName)
        //{
        //    string newname = "";
        //    Random r = new Random();
        //    int upperlimit = 20;

        //    if (peerName.Length != 0)
        //    {    
        //        newname = (peerName + r.Next(1, upperlimit).ToString());
        //    }
        //    else
        //    {
        //        newname = "Player" + r.Next(1, upperlimit).ToString();
        //    }

        //    return newname;
        //}

        public void peerDisconnect(string reason)
        {
            if (locoPeer != null)
            {
                locoPeer.Disconnect(reason);
            }
        }

        public List<string> peerDiscoverPeers()
        {
            locoPeer.discoverPeers(PORT, ref peersList);

            List<String> results = new List<String>();

            foreach (NetIncomingMessage connection in peersList)
            {
                string name = connection.ReadString();
                //if (name
                connection.Write(name);
                results.Add(name);
            }
            return results;
        }

        public void peerConnect(string peerName, int charNum)
        {
            //peerDiscoverPeers();
            if (peersList.Count != 0 && peerName.Trim() != "")
            {
                foreach (NetIncomingMessage connection in peersList)
                {
                    bool found = false;
                    string name = connection.ReadString();

                    if (name.Equals(peerName))
                    {
                        locoPeer.connectToPeer(connection, charNum);
                        
                        //Console.WriteLine("Connected to " + peerName);

                        connection.Write(name);
                        //connectedPeer = connection;
                        found = true;
                        challenger = connection;
                        challengername = name;
                    }

                    if (!found)
                    {
                        connection.Write(name);
                    }
                }
            }
        }

        public bool checkForConnection()
        {
            bool connected = false;
            List<NetConnection> connections = new List<NetConnection>();
            connections = locoPeer.getConnections();
            if (connections.Count() > 0)
                connected = true;

            return connected;
        }


        public bool checkForChallenge()
        {
            bool isThereAChallenge = false;

            if (locoPeer.NetWorker.challenger != null)
            {
                isThereAChallenge = true;
            }

            return isThereAChallenge;
        }

        public void sendChallenge(string opponent)
        {
            foreach (NetIncomingMessage connection in peersList)
            {
                string name = connection.ReadString();
                if (opponent == name)
                {
                    connection.Write(name);
                    locoPeer.sendMessage(Protocol.Type.challengeRequest, name, connection);
                }
                else
                {
                    connection.Write(name);
                }
            }
        }

        public void acceptChallenge()
        {
            locoPeer.NetWorker.challenger.SenderConnection.Approve();
        }

        public void denyChallenge()
        {
            locoPeer.NetWorker.challenger.SenderConnection.Deny("Sorry, not today");
        }


        public string returnChallengername()
        {
            string challengername = "";
            if (locoPeer.NetWorker.challenger != null)
            {
                challengername = locoPeer.NetWorker.challenger.ToString();
            }
            return challengername;
        }

        #endregion
    }
}
