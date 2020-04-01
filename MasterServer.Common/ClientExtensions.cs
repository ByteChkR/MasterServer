using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MasterServer.Common.Networking;

namespace MasterServer.Common
{
//    internal static class ClientExtensions
//    {
//        public static void SendPacket<T>(this TcpClient client, T structure) where T : struct
//        {
//            byte[] bytes = PacketHelper.StructToBytes(structure);
//            client.GetStream().Write(bytes, 0, bytes.Length);
//        }
//        public static void ReceivePacket<T>(this TcpClient client, ref T structure) where T : struct
//        {
//            byte[] bytes = new byte[Marshal.SizeOf(structure)];
//            client.GetStream().Read(bytes, 0, bytes.Length);
//            PacketHelper.BytesToStruct(bytes, ref structure);
//        }


//        public static Task SendPacketAsync<T>(this TcpClient client, T structure) where T : struct
//        {
//            byte[] bytes = PacketHelper.StructToBytes(structure);

//            return client.GetStream().WriteAsync(bytes, 0, bytes.Length);
//        }
//        public static Task<T> ReceivePacketAsync<T>(this TcpClient client, ref T structure) where T : struct
//        {
//            byte[] bytes = new byte[Marshal.SizeOf(structure)];
//            Task<T> t = new Task<T>(() => ReceiveAsync<T>(client, bytes));
//            t.Start();
//            return t;
//        }

//        private static T ReceiveAsync<T>(TcpClient client, byte[] bytes)where T:struct
//        {
//            client.GetStream().Read(bytes, 0, bytes.Length);
//            T ret = default(T);
//            PacketHelper.BytesToStruct(bytes, ref ret);
//            return ret;
//        }

//}
}