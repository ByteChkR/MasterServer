using System;
using System.Net.Sockets;
using Byt3.Serialization;
using MasterServer.Common;
using MasterServer.Common.Networking.Packets;

namespace MasterServer.Server.ConnectionManaging
{
    public class WaitingQueueItem : IComparable<WaitingQueueItem>
    {
        private TcpClient Client;
        private TimeSpan QueuedTime => DateTime.Now - StartQueueTime;
        public readonly string Identifier;

        private int HeartBeatsSent;
        private int MissedHeartBeats;
        private int MaxMissedHeartBeats;
        private int HeartBeatInterval;
        private DateTime LastHeartBeat;
        private readonly DateTime StartQueueTime;

        public bool IsConnected => Client.Connected;

        public bool ReceivedEndConnection()
        {
            return Client.Client.Poll(0, SelectMode.SelectRead);
            //if (/*!Byt3Serializer.TryReadPacket(Client.GetStream(), out object o)*/)
            //{
            //    return true;
            //}

            //return o is ServerExitPacket;
        }

        public WaitingQueueItem(int heartBeatInterval, int maxMissedHeartBeats, ClientHandshakePacket initPacket, TcpClient client)
        {
            HeartBeatInterval = heartBeatInterval;
            MaxMissedHeartBeats = maxMissedHeartBeats;
            StartQueueTime = LastHeartBeat = DateTime.Now;
            Client = client;

            Identifier = Client.Client.RemoteEndPoint.ToString();
            Logger.DefaultLogger("Client " + Identifier + " added to the Waiting Queue.");

            Byt3Serializer.TryWritePacket(Client.GetStream(), initPacket);

        }

        public QueueInfo GetQueueInfo()
        {
            return new QueueInfo { HeartbeatsSent = HeartBeatsSent, Name = Identifier, TimeInQueue = QueuedTime };
        }

        public bool ReceivedHeartbeat()
        {
            int millisSince = (int)(DateTime.Now - LastHeartBeat).TotalMilliseconds;
            if (millisSince <= HeartBeatInterval) return true; //Does not need to have heart beat this frame because server tick != heart beat tick


            while (Client.Available > 0)
            {
                try
                {
                    if (!Byt3Serializer.TryReadPacket(Client.GetStream(), out object o))
                    {
                        throw new Exception();
                    }
                    if (o is ClientHeartBeatPacket)
                    {
                        //SendHeartbeat();
                        HeartBeatsSent++;
                        LastHeartBeat = DateTime.Now;
                        MissedHeartBeats = 0;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
                
            }

            

            MissedHeartBeats++;
            if (MissedHeartBeats <= MaxMissedHeartBeats)
            {
                Logger.DefaultLogger("Client missed heart beat but is within tolerance.");
                return true;
            }

            return false;
        }

        public void CloseConnection()
        {

            Client.Close();

        }

        public void SendMatchFound(int port)
        {
            if (!Byt3Serializer.TryWritePacket(Client.GetStream(), new ClientInstanceReadyPacket() { Port = port })) throw new Exception("Serializer Write Error");
            //CloseConnection();
        }

        private void SendHeartbeat()
        {
            Byt3Serializer.TryWritePacket(Client.GetStream(), new ClientHeartBeatPacket());
        }

        public int CompareTo(WaitingQueueItem otherItem)
        {
            return QueuedTime.CompareTo(otherItem.QueuedTime);
        }
    }
}