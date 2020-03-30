using System;
using System.Runtime.InteropServices;

namespace MasterServer.Common
{
    public class PacketHelper
    {
        public static byte[] StructToBytes<T>(T obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];

            IntPtr handle = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(obj, handle, true);
            Marshal.Copy(handle, arr, 0, size);
            Marshal.FreeHGlobal(handle);

            return arr;
        }

        public static void BytesToStruct<T>(byte[] bytes, ref T outObj) where T : struct
        {


            int size = Marshal.SizeOf(outObj);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0, ptr, size);

            outObj = (T)Marshal.PtrToStructure(ptr, outObj.GetType());
            Marshal.FreeHGlobal(ptr);

        }
    }
}