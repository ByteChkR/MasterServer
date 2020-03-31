using System.IO;
using System.Runtime.InteropServices;

namespace MasterServer.Common.Networking.Packets.Serializers
{
    public class SimpleStructSerializer<T> : IPacketSerializer
        where T : struct
    {
        public object Deserialize(Stream s)
        {
            int size = Marshal.SizeOf<T>();
            T ret = default(T);
            byte[] bytes = new byte[size];
            s.Read(bytes, 0, bytes.Length);
            PacketHelper.BytesToStruct(bytes, ref ret);
            return ret;
        }

        public void Serialize(Stream s, object obj)
        {
            byte[] data = PacketHelper.StructToBytes((T)obj);
            s.Write(data, 0, data.Length);
        }
    }
}