using System.IO;
using System.Runtime.InteropServices;

namespace MasterServer.Common.Packets.Serializers
{
    public class ClientHeartBeatSerializer : SimpleStructSerializer<ClientHeartBeatPacket>
    {
    }

    public class ClientInstanceReadySerializer : SimpleStructSerializer<ClientInstanceReadyPacket>
    {
    }
    public class ClientHandshakeSerializer : SimpleStructSerializer<ClientHandshakePacket>
    {
    }
}