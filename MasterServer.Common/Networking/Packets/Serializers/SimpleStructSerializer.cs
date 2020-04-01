using System.IO;
using System.Runtime.InteropServices;
using Byt3.Serialization;

namespace MasterServer.Common.Networking.Packets.Serializers
{
    public class SimpleStructSerializer<T> : ATSerializer<T>
        where T : struct
    {
        public override T DeserializePacket(Stream s)
        {
            int size = Marshal.SizeOf<T>();
            T ret = default(T);
            byte[] bytes = new byte[size];
            s.Read(bytes, 0, bytes.Length);
            PacketHelper.BytesToStruct(bytes, ref ret);
            return ret;
        }

        public override void SerializePacket(Stream s, T obj)
        {
            byte[] data = PacketHelper.StructToBytes(obj);
            s.Write(data, 0, data.Length);
        }
    }
}