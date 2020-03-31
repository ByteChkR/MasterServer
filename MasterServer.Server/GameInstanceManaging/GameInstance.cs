using System;
using System.Diagnostics;
using MasterServer.Common;
using MasterServer.Server.PortManaging;

namespace MasterServer.Server.GameInstanceManaging
{
    public class GameInstance
    {
        public bool Running { get; private set; }
        public readonly DateTime StartTime;
        public TimeSpan TimeRunning => DateTime.Now - StartTime;

        public readonly Port AssociatedPort;
        private readonly Process GameInstanceProcess;
        private readonly GameInstanceManager Manager;

        internal GameInstance(Port port, GameInstanceManager manager, Process process)
        {
            GameInstanceProcess = process;
            GameInstanceProcess.EnableRaisingEvents = true;
            GameInstanceProcess.Exited += GameInstanceProcess_Exited;
            AssociatedPort = port;
            Manager = manager;
            Running = true;
            StartTime = DateTime.Now;
            Logger.DefaultLogger("Initializing Game Instance on Port: " + AssociatedPort.PortNum);
        }

        private void GameInstanceProcess_Exited(object sender, EventArgs e)
        {
            Logger.DefaultLogger("Game MatchMakingServer Instance Process Exited");

            Manager.InstanceExited(this);
            AssociatedPort.Free();
        }

        public GameInstanceInfo GetInstanceInfo()
        {
            return new GameInstanceInfo { Name = GameInstanceProcess.ProcessName, Port = AssociatedPort.PortNum, UpTime = TimeRunning };
        }

        internal void Stop()
        {
            if (GameInstanceProcess.HasExited) return;
            Logger.DefaultLogger("Stopping Game Instance");
            Running = false;
            GameInstanceProcess.Kill();
        }

        internal void Dispose()
        {
            if (Running) Stop();

            Logger.DefaultLogger("Disposing Game Instance");
            GameInstanceProcess.Dispose();
        }

    }
}