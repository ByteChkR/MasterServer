using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MasterServer.Common;

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
            string args = $"--closeOnMatchEnded --no-player-timeout:{noPlayerTimeout} --server-timeout:{serverTimeout} --port:{port}";
            ProcessStartInfo psi = new ProcessStartInfo(path, args);
            psi.WorkingDirectory = workingDir;
            //psi.CreateNoWindow = true;
            Process p = new Process();
            p.StartInfo = psi;

            Logger.DefaultLogger("Starting Server...");
            p.Start();
            return p;
        }

        private static void DumpOutput(string output)
        {
            Logger.DefaultLogger(output);
        }

        private static void InitGameInstance(ClientSession[] sessions, int port)
        {
            for (int i = 0; i < sessions.Length; i++)
            {
                sessions[i].SetInstanceReady(true, port);
            }
        }
        #endregion

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
                    Logger.DefaultLogger("Remove Inactive Game Instance on port: " + gameInstance.Key);
                    rems.Add(gameInstance.Key);
                    Logger.DefaultLogger("Remaining Instances: " + (GameInstances.Count - rems.Count) + "/" + GameInstances.Count);
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
