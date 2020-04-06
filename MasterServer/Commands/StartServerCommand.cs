using Byt3.ADL;
using Byt3.CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer.Commands
{
    public class StartServerCommand : AbstractCommand
    {
        public StartServerCommand() :
            base( new[] { "--start-server", "-start" }, "Starts the MatchMakingServer",
                false)
        {
            CommandAction = StartServer;
        }


        private void StartServer(StartupArgumentInfo info, string[] args)
        {
            if (Program.MatchMaker == null) Program.MatchMaker = new MatchMakingServer(Program.Settings);

            if (!Program.MatchMaker.IsRunning)
            {
                Logger.Log(LogType.Log, "Starting Server...");
                Program.MatchMaker.StartServer();
            }
            else
            {
                Logger.Log(LogType.Log, "Server already started.");
            }

        }
    }
}