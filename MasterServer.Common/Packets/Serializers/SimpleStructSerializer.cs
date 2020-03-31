using System.IO;
using System.Runtime.InteropServices;

namespace MasterServer.Common.Packets.Serializers
{
    public class SimpleStructSerializer<T> : IPacketSerializer
        where T : struct
    {

        private static readonly int PacketSize = Marshal.SizeOf<T>();

        public object Deserialize(Stream s)
        {
            byte[] data = new byte[PacketSize];
            s.Read(data, 0, data.Length);
            T ret = default(T);
            PacketHelper.BytesToStruct(data, ref ret);
            return ret;
        }

        public void Serialize(Stream s, object obj)
        {
            byte[] data = PacketHelper.StructToBytes((T)obj);
            s.Write(data, 0, data.Length);
        }
    }
}