using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MasterServer.Common;
using Process = System.Diagnostics.Process;

namespace MasterServer.Server
{

    public class InstanceManager
    {
        private MasterServer Master;
        private Dictionary<int, Process> GameInstances = new Dictionary<int, Process>();
        public int InstanceCount => GameInstances.Count;
        private bool[] BlockedPorts;
        private int NextPort;

        public InstanceManager(MasterServer master)
        {
            Logger.DefaultLogger("Initializing Instance Master Server");
            Master = master;
            BlockedPorts = new bool[master.Settings.GameServerPortRange.Range];
        }

        public void StopGameInstances(int[] ports)
        {
            foreach (KeyValuePair<int, Process> gameInstance in GameInstances)
            {
                if (ports.Contains(gameInstance.Key))
                {
                    Logger.DefaultLogger("Killing Server On Port: " + gameInstance.Key);
                    gameInstance.Value.Kill();
                }
            }
        }

        public void CloseAll()
        {
            foreach (KeyValuePair<int, Process> gameInstance in GameInstances)
            {
                gameInstance.Value.Kill();
            }
        }

        #region Private
        private int GetNextPort()
        {
            for (int i = 0; i < BlockedPorts.Length; i++)
            {
                if (!BlockedPorts[i])
                {
                    return Master.Settings.GameServerPortRange.Min + i;
                }
            }

            throw new Exception("Not Enough Ports assigned.");
        }




        private Process StartGameInstanceServer(int port)
        {

            string path = Master.Settings.GameServerPath;
            string workingDir = Path.GetDirectoryName(path);
            bool exi = File.Exists(path);
            int serverTimeout = 3600 * 1000;
            int noPlayerTimeout = 25 * 1000;
            string args = $"Game.HeadlessInfo.CloseOnMatchEnded:true Game.HeadlessInfo.NoPlayerTimeout:{noPlayerTimeout} Game.HeadlessInfo.Timeout:{serverTimeout} Game.GameNetworkInfo.DefaultAddress.Port:{port}";
            ProcessStartInfo psi = new ProcessStartInfo(path, args);
            psi.WorkingDirectory = workingDir;
            //psi.CreateNoWindow = true;
            Process p = new Process();
            p.StartInfo = psi;

            Logger.DefaultLogger("Starting Game Server on Port: " + port);
            p.Start();
            return p;
        }

        private static void DumpOutput(string output)
        {
            Logger.DefaultLogger(output);
        }

        private static void InitGameInstance(ClientSession[] sessions, int port)
        {
            Thread.Sleep(5000);
            for (int i = 0; i < sessions.Length; i++)
            {
                sessions[i].SetInstanceReady(true, port);
            }
        }
        #endregion

        public ServerInstanceInfo[] GetServerInstanceInfos()
        {
            ServerInstanceInfo[] ret = new ServerInstanceInfo[BlockedPorts.Length];
            lock (GameInstances)
            {
                for (int i = 0; i < BlockedPorts.Length; i++)
                {
                    int port = Master.Settings.GameServerPortRange.Min + i;
                    if (!BlockedPorts[i])
                    {
                        ret[i] = new ServerInstanceInfo { Name = "UNUSED", Port = port, UpTime = "NONE" };
                    }
                    else
                    {
                        ret[i] = new ServerInstanceInfo
                        {
                            Name = GameInstances[port].ProcessName,
                            Port = port,
                            UpTime =
                            (GameInstances[port].StartTime - DateTime.UtcNow).ToString()
                        };
                    }
                }
            }

            return ret;
        }


        public bool CanCreateGame()
        {
            for (int i = 0; i < BlockedPorts.Length; i++)
            {
                if (!BlockedPorts[i]) return true;
            }

            return false;
        }

        public void CreateGame(ClientSession[] sessions)
        {
            int port = GetNextPort();
            BlockedPorts[port - Master.Settings.GameServerPortRange.Min] = true;
            Process p = StartGameInstanceServer(port);
            GameInstances.Add(port, p);
            InitGameInstance(sessions, port);
        }

        public void RemoveInactiveGameInstances()
        {
            List<int> rems = new List<int>();
            foreach (KeyValuePair<int, Process> gameInstance in GameInstances)
            {
                if (gameInstance.Value.HasExited)
                {
                    rems.Add(gameInstance.Key);
                    Logger.DefaultLogger("Remove Instance on Port:" + gameInstance.Key + " Remaining Instances: " + (Master.Settings.GameServerPortRange.Range - rems.Count) + "/" + Master.Settings.GameServerPortRange.Range);
                    BlockedPorts[gameInstance.Key - Master.Settings.GameServerPortRange.Min] = false;
                }
            }

            for (int i = 0; i < rems.Count; i++)
            {
                GameInstances.Remove(rems[i]);
            }
        }
    }
}
