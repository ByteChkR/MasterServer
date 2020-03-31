using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MasterServer.Common.Networking
{
    public class NetworkPackageStreamWrapper
    {
        private Stream stream;
        private List<byte> packetCache = new List<byte>();

        public NetworkPackageStreamWrapper(Stream s)
        {
            stream = s;
        }

        public int ReadInt()
        {
            byte[] buf = new byte[sizeof(int)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToInt32(buf, 0);
        }

        public uint ReadUInt()
        {
            byte[] buf = new byte[sizeof(uint)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToUInt32(buf, 0);
        }

        public long ReadLong()
        {
            byte[] buf = new byte[sizeof(long)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToInt64(buf, 0);
        }

        public ulong ReadULong()
        {
            byte[] buf = new byte[sizeof(ulong)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToUInt64(buf, 0);
        }

        public short ReadShort()
        {
            byte[] buf = new byte[sizeof(short)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToInt16(buf, 0);
        }
        public ushort ReadUShort()
        {
            byte[] buf = new byte[sizeof(ushort)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToUInt16(buf, 0);
        }

        public bool ReadBool()
        {
            byte[] buf = new byte[sizeof(bool)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToBoolean(buf, 0);
        }

        public float ReadFloat()
        {
            byte[] buf = new byte[sizeof(float)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToSingle(buf, 0);
        }

        public double ReadDouble()
        {
            byte[] buf = new byte[sizeof(double)];
            stream.Read(buf, 0, buf.Length);
            return BitConverter.ToDouble(buf, 0);
        }

        public string ReadString()
        {
            int len = ReadInt();
            byte[] buf = new byte[len];
            stream.Read(buf, 0, buf.Length);
            return Encoding.ASCII.GetString(buf);
        }

        public byte[] ReadBytes()
        {
            int len = ReadInt();
            byte[] buf = new byte[len];
            stream.Read(buf, 0, buf.Length);
            return buf;
        }


        public int Write(byte[] value)
        {
           int w= Write(value.Length);
            packetCache.AddRange(value);
            return value.Length+ w;
        }

        public int Write(int value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            packetCache.AddRange(buf);
            return buf.Length;
        }
        public int Write(bool value)
        {

            byte[] buf = BitConverter.GetBytes(value);
            packetCache.AddRange(buf);
            return buf.Length;
        }
        public int Write(float value)
        {

            byte[] buf = BitConverter.GetBytes(value);
            packetCache.AddRange(buf);
            return buf.Length;
        }
        public int Write(double value)
        {
            byte[] buf = BitConverter.GetBytes(value);
            packetCache.AddRange(buf);
            return buf.Length;

        }
        public int Write(string value)
        {
            byte[] buf = Encoding.ASCII.GetBytes(value);
            int w = Write(buf.Length);
            packetCache.AddRange(buf);
            return buf.Length + w;
        }

        


        public void CompleteWrite()
        {
            stream.Write(packetCache.ToArray(), 0, packetCache.Count);
            packetCache.Clear();
        }

    }
}