using System;
using System.Linq;
using MasterServer.Common;

namespace MasterServer.Server.PortManaging
{
    public class PortManager
    {
        private Port[] PortMap;
        private MatchMakerSettings Settings;
        public PortManager(MatchMakerSettings settings)
        {
            Settings = settings;
            PortMap = new Port[Settings.GameServerPortRange.Range];
            InitializePortMap();
            Logger.DefaultLogger("Initializing Port Manager on Range: " + Settings.GameServerPortRange);
        }

        private void InitializePortMap()
        {
            for (int i = 0; i < PortMap.Length; i++)
            {
                PortMap[i] = new Port(Settings.GameServerPortRange.StartPort + i);
            }
        }

        public bool HasFreePorts()
        {
            return PortMap.Any(port => !port.Acquired);
        }

        public Port AquirePort()
        {
            if (!HasFreePorts()) throw new InvalidOperationException("No Free Ports available.");
            for (int i = 0; i < PortMap.Length; i++)
            {
                if (!PortMap[i].Acquired)
                {
                    Port p = PortMap[i];

                    Logger.DefaultLogger("Aquired Port: " + p.PortNum);

                    p.Aquire();
                    return p;
                }
            }
            throw new InvalidOperationException("No Free Ports available. Invalid State");
        }



    }
}