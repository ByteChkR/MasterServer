using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MasterServer.Common;
using MasterServer.Server.PortManaging;

namespace MasterServer.Server.GameInstanceManaging
{
    public class GameInstanceManager
    {

        private readonly ProcessStartInfo StartInfo;
        private readonly MatchMakerSettings Settings;
        public int InstanceCount { get; private set; }
        private List<GameInstance> GameInstances = new List<GameInstance>();

        public GameInstanceManager(MatchMakerSettings settings)
        {
            Logger.DefaultLogger("Initializing Game Instance Manager");

            Settings = settings;
            StartInfo = new ProcessStartInfo(Settings.GameServerPath);
            StartInfo.CreateNoWindow = settings.InstanceCreateNoWindow;
            StartInfo.UseShellExecute = settings.InstanceUseShellExecute;
            StartInfo.WorkingDirectory = Path.GetDirectoryName(Settings.GameServerPath);
        }

        public GameInstance StartInstance(Port port, string args)
        {
            Process p = new Process();
            p.StartInfo = StartInfo;
            p.StartInfo.Arguments = args;


            Logger.DefaultLogger("Starting Game MatchMakingServer Instance on Port: " + port.PortNum);
            Logger.DefaultLogger("Starting Game MatchMakingServer Instance with Arguments: " + args);

            p.Start();
            GameInstance g = new GameInstance(port, this, p);
            GameInstances.Add(g);
            InstanceCount++;


            return g;
        }

        public GameInstanceInfo[] GetInstanceInfos()
        {
            GameInstanceInfo[] ret = new GameInstanceInfo[GameInstances.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = GameInstances[i].GetInstanceInfo();
            }

            return ret;
        }

        public void CloseInstances(int[] ports)
        {
            for (int i = 0; i < GameInstances.Count; i++)
            {
                if (ports.Contains(GameInstances[i].AssociatedPort.PortNum))
                    GameInstances[i].Stop();
            }
        }

        public void CloseAllInstances()
        {
            for (int i = 0; i < GameInstances.Count; i++)
            {
                GameInstances[i].Stop();
            }
        }

        internal void InstanceExited(GameInstance instance)
        {
            GameInstances.Remove(instance);

            InstanceCount--;

            instance.Dispose();
        }
    }
}