using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading;

namespace Locomotion.Networking
{
    public class LocoPeer
    {
        private static NetPeer peer;
        public string name;

        public PeerListener NetWorker;
        public Thread NetThread;

        public LocoPeer(string signature, string name, int port)
        {
            this.name = name;
            NetPeerConfiguration config = new NetPeerConfiguration(signature);
            config.AutoFlushSendQueue = true;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            
            config.ConnectionTimeout = 10;
            
            config.Port = port;

           
            
            peer = new NetPeer(config);

            peer.Start();

            NetWorker = new PeerListener(peer, name);
            NetThread = new Thread(NetWorker.ProcessNet);
            NetThread.Start();

          
        }

        public List<NetConnection> getConnections()
        {
            return peer.Connections;
        }

        public void sendMove(string playerName, int fromRow, int fromCol, int toRow, int toCol)
        {
            NetOutgoingMessage move = peer.CreateMessage();
            move.Write((int)Protocol.Type.move);
            move.Write(playerName);
            move.Write("Locomotion");



            move.Write((int)fromRow);
            move.Write((int)fromCol);
            move.Write((int)toRow);
            move.Write((int)toCol);

            peer.SendMessage(move, NetWorker.challenger.SenderConnection, NetDeliveryMethod.ReliableOrdered);
        }

        public bool sendMessage(Protocol.Type protocol, string text, NetIncomingMessage recipient)
        {
            NetOutgoingMessage om = peer.CreateMessage();
            om.Write((int) protocol);
            om.Write(text);
            peer.SendMessage(om, recipient.SenderConnection, NetDeliveryMethod.ReliableOrdered);
            //peer.sen
            peer.FlushSendQueue();
            return true;
        }

        public void connectToPeer(NetIncomingMessage recipient, int charNum)
        {
            NetOutgoingMessage name = peer.CreateMessage(this.name);
            name.Write((int)charNum);

            peer.Connect(recipient.SenderEndPoint.Address.ToString(), recipient.SenderEndPoint.Port, name);
            //this.NetWorker.challenger = recipient;
        }

        //Obtains a list of found servers
        public void discoverPeers(int port, ref List<NetIncomingMessage> peersList)
        {
            NetWorker.foundPeers.Clear();

            peer.DiscoverLocalPeers(port);
            //Thread.Sleep(1000);
            //List must have something
            peersList = NetWorker.foundPeers;
        }

        public void Disconnect(string reason)
        {
            peer.Shutdown(reason);
            this.NetWorker.shouldQuit = true;
        }
    }
}
