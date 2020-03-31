namespace MasterServer.Common.Networking.Packets
{
    public struct ClientHandshakePacket
    {
        public int MaxInstances;
        public int CurrentInstances;
        public int WaitingQueue;
        public int HeartBeat;
    }
}