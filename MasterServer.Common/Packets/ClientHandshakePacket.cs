using System;

namespace MasterServer.Common.Packets
{
    public struct ClientHandshakePacket
    {
        public int MaxInstances;
        public int CurrentInstances;
        public int WaitingQueue;
        public int HeartBeat;
        public int ServerVersionMajor;
        public int ServerVersionMinor;
        public int ServerRevisionMajor;
        public int ServerRevisionMinor;
    }
}