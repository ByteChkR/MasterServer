using System;
using System.Net.Sockets;
using MasterServer.Common;
using MasterServer.Common.Networking;
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

        public WaitingQueueItem(int heartBeatInterval, int maxMissedHeartBeats, ClientHandshakePacket initPacket, TcpClient client)
        {
            HeartBeatInterval = heartBeatInterval;
            MaxMissedHeartBeats = maxMissedHeartBeats;
            StartQueueTime = LastHeartBeat = DateTime.Now;
            Client = client;

            Identifier = Client.Client.RemoteEndPoint.ToString();
            Logger.DefaultLogger("Client " + Identifier + " added to the Waiting Queue.");

            PacketSerializer.Serializer.WritePacket(Client.GetStream(), initPacket);
            
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
                object o = PacketSerializer.Serializer.GetPacket(Client.GetStream());
                if (o is ClientHeartBeatPacket)
                {
                    //SendHeartbeat();
                    HeartBeatsSent++;
                    LastHeartBeat = DateTime.Now;
                    MissedHeartBeats = 0;
                    return true;
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
            PacketSerializer.Serializer.WritePacket(Client.GetStream(), new ClientInstanceReadyPacket() { Port = port });
            //CloseConnection();
        }

        private void SendHeartbeat()
        {
            PacketSerializer.Serializer.WritePacket(Client.GetStream(), new ClientHeartBeatPacket());
        }

        public int CompareTo(WaitingQueueItem otherItem)
        {
            return QueuedTime.CompareTo(otherItem.QueuedTime);
        }
    }
}