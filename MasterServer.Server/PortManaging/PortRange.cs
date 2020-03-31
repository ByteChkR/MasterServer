using System;

namespace MasterServer.Server.PortManaging
{
    public struct PortRange
    {
        private const int MaxPort = ushort.MaxValue;
        public  int StartPort;
        public  int EndPort;
        public int Range => EndPort - StartPort;

        public PortRange(int startPort, int endPort)
        {
            if (startPort <= 0 || startPort > MaxPort) throw new ArgumentOutOfRangeException("startPort", "Has to be between 1 and 65535");
            if (endPort <= 1 || endPort > MaxPort) throw new ArgumentOutOfRangeException("endPort", "Has to be between 2 and 65535");
            if (startPort >= endPort) throw new InvalidOperationException("Start Port has to be smaller than End Port.");

            StartPort = startPort;
            EndPort = endPort;
        }

        public override string ToString()
        {
            return $"StartPort: {StartPort} EndPort: {EndPort} Range: {Range}";
        }
    }
}