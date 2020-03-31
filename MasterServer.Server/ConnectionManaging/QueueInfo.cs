using System;

namespace MasterServer.Server.ConnectionManaging
{
    public struct QueueInfo
    {
        public string Name;
        public int HeartbeatsSent;
        public TimeSpan TimeInQueue;
    }
}