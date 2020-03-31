using System.IO;

namespace MasterServer.Common.Networking
{
    public interface IPacketSerializer
    {
        object Deserialize(Stream s);
        void Serialize(Stream s, object obj);
    }
}