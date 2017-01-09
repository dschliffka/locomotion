using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Windows;
using System.Threading;
using System.Xml.Serialization;

namespace Locomotion.Networking
{
    public class PeerListener
    {
        public event MessageEventHandler messageReceived;
        public event MoveEventHandler moveReceived;
        public event ConnectionEventHandler connectionStatusReceived;
        public event ChallengeEventHandler challengeReceived;
        public event OpponentEventHandler opponentDetected;

        public bool goFirst = false;


        public NetPeer peer = null;
        public List<NetIncomingMessage> foundPeers;

        public volatile bool shouldQuit = false;

        private string name;

        public NetIncomingMessage challenger;

        public bool allowDiscovery = true;

        public PeerListener(NetPeer inServer, string name)
        {
            foundPeers = new List<NetIncomingMessage>();
            peer = inServer;
            this.name = name;

            challenger = null;
        }

        protected virtual void onOpponentDetected(OpponentEventArgs opponent)
        {
            if (opponentDetected != null) opponentDetected(this, opponent);
        }

        protected virtual void onChallengeReceived(ChallengeEventArgs challenge)
        {
            if (challengeReceived != null) challengeReceived(this, challenge);
        }


        protected virtual void onMessageReceived(MessageEventArgs message)
        {
            if (messageReceived != null) messageReceived(this, message);
        }


        protected virtual void onMoveReceived(MoveEventArgs arguments)
        {
            if (moveReceived != null) moveReceived(this, arguments);
        }

        protected virtual void onDisconnected(ConnectionEventArgs status)
        {
            if (connectionStatusReceived != null) connectionStatusReceived(this, status);
        }

        protected virtual void onConnected(ConnectionEventArgs status)
        {
            if (connectionStatusReceived != null) connectionStatusReceived(this, status);
        }

        private string serializeObject(object toSerialize)
        {
            StringWriter outStream = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(toSerialize.GetType());
            serializer.Serialize(outStream, toSerialize);
            string serializedObject = outStream.ToString();
            return serializedObject;
        }

        private object deserializeObject(string toDeserialize, object typeOfObject)
        {
            StringReader inStream = new StringReader(toDeserialize);
            XmlSerializer deserializer = new XmlSerializer(typeOfObject.GetType());
            return (deserializer.Deserialize(inStream));
        }

        //public string addRandToName(string name)
        //{
        //    if (!string.IsNullOrEmpty(name))
        //        if (name.Trim() != "")
        //        {
        //            string newname = name;
        //            //do something
        //            return newname;
        //        }
        //}


        private bool isNewOpponent(string opponentName, string opponentId)
        {
            bool result = true;

            //peer.co


            return result;

        }




        
        public void ProcessNet()
        {
            // read messages
            while (!shouldQuit)
            {
                NetIncomingMessage msg;
                while ((msg = peer.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        //Request from other peers
                        case NetIncomingMessageType.DiscoveryRequest:
                            if (allowDiscovery)
                            {
                                NetOutgoingMessage response = peer.CreateMessage();
                                response.Write(name);
                                response.Write(peer.UniqueIdentifier.ToString());

                                //Send the response to the sender of the request
                                peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
                            }
                            break;
                        case NetIncomingMessageType.DiscoveryResponse:          
                            string peerName = msg.ReadString();
                            string peerUniqueId;

                            try
                            {
                                peerUniqueId = msg.ReadString();
                            }
                            catch (Exception error)
                            {
                                peerUniqueId = "HighNoon";
                            }

                            
                            string myId = peer.UniqueIdentifier.ToString(); //returns "random" no.
                                if (myId != peerUniqueId) // if (name == peerName)
                                {
                                    msg.Write(peerName);
                                    msg.Write(peerUniqueId);

                                    if (isNewOpponent(peerName, peerUniqueId))
                                    { 
                                        
                                    };


                                    foundPeers.Add(msg);

                                    OpponentEventArgs opponent = new OpponentEventArgs();
                                    opponent.opponentName = peerName;

                                    if (peerUniqueId == "HighNoon")
                                    {
                                        NetOutgoingMessage response = peer.CreateMessage();
                                        response.Write(name);
                                        peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
                                    }

                                    
                                    onOpponentDetected(opponent);
                                }
                            break;
                        case NetIncomingMessageType.ConnectionApproval: //This means that we are being challenged

                            if (allowDiscovery)
                            {
                                int charNum = 0;
                                string challengerName = msg.ReadString();
                                try
                                {
                                    charNum = msg.ReadInt32();
                                }
                                catch( Exception ex)
                                { 
                                
                                }



                                ChallengeEventArgs receivedChallenge = new ChallengeEventArgs();
                                receivedChallenge.challengerName = challengerName;
                                receivedChallenge.characterNumber = charNum;
                                msg.Write(challengerName);
                                challenger = msg;
                                onChallengeReceived(receivedChallenge);


                            }
                            else
                            {
                                msg.SenderConnection.Deny();
                            }

                            break;
                        case NetIncomingMessageType.StatusChanged:

                           //Got rid of if statements here

                                NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                                ConnectionEventArgs connectionStatus = new ConnectionEventArgs();

                                if (status == NetConnectionStatus.Connected)
                                {
                                    challenger = msg;
                                    allowDiscovery = false;

                                    connectionStatus.connected = true;
                                    connectionStatus.message = "Connected";
                                    onConnected(connectionStatus);


                                    NetOutgoingMessage piece = peer.CreateMessage();
                                    if (goFirst)
                                    {
                                        piece.Write((int)6);
                                        piece.Write("Light");
                                        
                                    }
                                    else
                                    {
                                        piece.Write((int)6);
                                        piece.Write("Dark");

                                    }
                                    piece.Write(name);

                                    peer.SendMessage(piece, challenger.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                    //MessageBox.Show("You are Connected!");
                                }

                                if (status == NetConnectionStatus.Disconnected)
                                {
                                    string reason = msg.ReadString();
                                    allowDiscovery = true;

                                    connectionStatus.connected = false;
                                    connectionStatus.message = reason;
                                    onDisconnected(connectionStatus);
                                    //MessageBox.Show("Something happened :( " + reason);
                                }

                           
                            break;

                        case NetIncomingMessageType.Data:
                            //Reads what kind of protocol inside the message received
                            int command = msg.ReadInt32();

                            // Message transmission
                            if (command == (int)Protocol.Type.chatMessage)
                            {
                                MessageEventArgs message = new MessageEventArgs();
                                message.text = msg.ReadString();
                                onMessageReceived(message);

                                //MessageBox.Show("Message: " + message.text); 
                            }

                            // Move transmission
                            if (command == (int)Protocol.Type.move)
                            {
                                string playerName = msg.ReadString();
                                string gameType = msg.ReadString();

                                int fromRow;
                                int fromCol;
                                int toRow;
                                int toCol;

                                //Trying for Locomotion
                                if(gameType == "Locomotion")
                                {
                                    fromRow = msg.ReadInt32();
                                    fromCol = msg.ReadInt32();
                                    toRow = msg.ReadInt32();
                                    toCol = msg.ReadInt32();

                                }
                                else //If not 
                                {
                                    string HighNoonMove = msg.ReadString();
                                    fromRow = 6 - int.Parse(HighNoonMove[0].ToString());
                                    fromCol = int.Parse(HighNoonMove[1].ToString());
                                    
                                    
                                    toRow = 6 - int.Parse(HighNoonMove[2].ToString());
                                    toCol = int.Parse(HighNoonMove[3].ToString());
                                    
                                }



                                MoveEventArgs move = new MoveEventArgs();
                                move.playerName = playerName;
                                move.fromRow = fromRow;
                                move.fromCol = fromCol;
                                move.toRow = toRow;
                                move.toCol = toCol;
                                onMoveReceived(move);

                                //MessageBox.Show("From:" + fromRow.ToString() + fromCol.ToString() + " to " + toRow.ToString() + toCol.ToString()); 
                            }

                            break;
                        default:
                            //
                            break;
                    }
                }
            }
            //Console.WriteLine("Exiting net thread!");
        }
    }
}