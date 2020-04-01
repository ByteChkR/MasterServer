using Byt3.Serialization;

namespace MasterServer.Common.Networking.Packets.Serializers
{
    public abstract class SimpleStructSerializer<T> : ASerializer<T>
        where T : struct
    {
        public override T DeserializePacket(PrimitiveValueWrapper s)
        {
            T ret = default(T);
            byte[] bytes = s.ReadBytes();

            PacketHelper.BytesToStruct(bytes, ref ret);
            return ret;
        }

        public override void SerializePacket(PrimitiveValueWrapper s, T obj)
        {
            byte[] data = PacketHelper.StructToBytes(obj);
            s.Write(data);
        }
    }
}