using Byt3.CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer.Commands
{
    public class StartServerCommand : AbstractCommand
    {
        public StartServerCommand() :
            base(StartServer, new[] { "--start-server", "-start" }, "Starts the MatchMakingServer",
                false)
        {

        }


        private static void StartServer(StartupInfo info, string[] args)
        {
            if (Program.MatchMaker == null) Program.MatchMaker = new MatchMakingServer(Program.Settings);

            if (!Program.MatchMaker.IsRunning)
            {
                Logger.DefaultLogger("Starting Server...");
                Program.MatchMaker.StartServer();
            }
            else
            {
                Logger.DefaultLogger("Server already started.");
            }

        }
    }
}