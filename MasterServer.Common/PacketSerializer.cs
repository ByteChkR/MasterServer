using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MasterServer.Common
{
    public interface IPacketSerializer
    {
        object Deserialize(Stream s);
        void Serialize(Stream s, object obj);
    }

    public class PacketSerializer
    {
        public static readonly PacketSerializer Serializer = new PacketSerializer();
        private class NetworkPacket
        {
            public const int PacketMaxSize = ushort.MaxValue;
            public string PacketType;
            public byte[] Payload;

            public NetworkPacket(string packetType, byte[] payload)
            {
                PacketType = packetType;
                Payload = payload;
            }
        }

        private class NetworkPacketSerializer : IPacketSerializer
        {
            public Type PacketType => typeof(NetworkPacket);

            public object Deserialize(Stream s)
            {
                byte[] lenBytes = new byte[sizeof(ushort) * 2];
                s.Read(lenBytes, 0, lenBytes.Length);
                ushort len = BitConverter.ToUInt16(lenBytes, 0);
                ushort packetNameLen = BitConverter.ToUInt16(lenBytes, sizeof(ushort));
                ushort payloadLen = (ushort)(len - packetNameLen);
                byte[] str = new byte[packetNameLen];
                s.Read(str, 0, str.Length);
                byte[] payload = new byte[payloadLen];
                s.Read(payload, 0, payload.Length);
                return new NetworkPacket(Encoding.ASCII.GetString(str), payload);

            }

            public void Serialize(Stream s, object obj)
            {
                NetworkPacket packet = (NetworkPacket)obj;
                byte[] str = Encoding.ASCII.GetBytes(packet.PacketType);
                List<byte> c = new List<byte>();
                c.AddRange(BitConverter.GetBytes((ushort)(str.Length + packet.Payload.Length)));
                c.AddRange(BitConverter.GetBytes((ushort)str.Length));
                c.AddRange(str);
                c.AddRange(packet.Payload);
                s.Write(c.ToArray(), 0, c.Count);
            }
        }

        private readonly Dictionary<string, IPacketSerializer> Serializers;

        public PacketSerializer()
        {
            Serializers = new Dictionary<string, IPacketSerializer>();
            NetworkPacketSerializer ser = new NetworkPacketSerializer();
            AddSerializer(ser, typeof(NetworkPacket));
        }

        public object GetPacket(Stream s)
        {
            string t = typeof(NetworkPacket).AssemblyQualifiedName;
            NetworkPacket packet = (NetworkPacket)Serializers[t].Deserialize(s);
            
            MemoryStream ms = new MemoryStream(packet.Payload);
            object obj = Serializers[packet.PacketType].Deserialize(ms);
            ms.Close();
            return obj;
        }

        public void WritePacket(Stream s, object obj)
        {
            MemoryStream ms = new MemoryStream();
            string t = obj.GetType().AssemblyQualifiedName;
            Serializers[t].Serialize(ms, obj);
            ms.Position = 0;
            byte[] b = new byte[ms.Length];
            ms.Read(b, 0, b.Length);
            ms.Close();
            Write(s, new NetworkPacket(t, b));
        }

        private void Write(Stream s, NetworkPacket packet)
        {
            string t = packet.GetType().AssemblyQualifiedName;
            Serializers[t].Serialize(s, packet);
        }

        public void AddSerializer(IPacketSerializer serializer, Type t)
        {
            if (Serializers.ContainsKey(t.AssemblyQualifiedName)) return;

            Logger.DefaultLogger("Adding Serializer for type: " + t.AssemblyQualifiedName);

            Serializers.Add(t.AssemblyQualifiedName, serializer);
        }

    }
}