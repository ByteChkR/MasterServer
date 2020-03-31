using System;

namespace MasterServer.Server.PortManaging
{
    public class Port
    {
        public readonly int PortNum;
        public bool Acquired { get; private set; }

        internal Port(int port)
        {
            PortNum = port;
            Acquired = false;
        }

        public void Free()
        {
            if (!Acquired) throw new InvalidOperationException("Can not Free a port that is not Aquired.");
            Acquired = false;
        }


        internal void Aquire()
        {
            if (Acquired) throw new InvalidOperationException("Can not Aquire a port that is not Free.");
            Acquired = true;
        }
    }
}