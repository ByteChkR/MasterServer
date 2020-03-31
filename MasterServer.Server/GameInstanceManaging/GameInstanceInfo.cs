using System;

namespace MasterServer.Server.GameInstanceManaging
{
    public struct GameInstanceInfo
    {
        public string Name;
        public TimeSpan UpTime;
        public int Port;
    }
}