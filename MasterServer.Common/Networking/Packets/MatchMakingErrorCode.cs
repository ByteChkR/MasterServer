namespace MasterServer.Common.Networking.Packets
{
    public enum MatchMakingErrorCode
    {
        None,
        ClientDisconnectDuringReady,
        ClientQueueAborted,
        WrongPacketReceived,
        PacketSerializationException,
        PacketDeserializationException,
        SocketException,
        UnhandledError
    }

}