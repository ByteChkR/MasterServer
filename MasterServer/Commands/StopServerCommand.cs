using CommandRunner;
using MasterServer.Common;

namespace MasterServer.Commands
{
    public class StopServerCommand : AbstractCommand
    {
        public StopServerCommand() :
            base(StopServer, new[] { "--stop-server", "-stop" }, "Stops the MatchMakingServer",
                false)
        {

        }


        private static void StopServer(StartupInfo info, string[] args)
        {
            if(Program.MatchMaker.IsRunning)
            {
                Logger.DefaultLogger("Stopping Server...");
                Program.MatchMaker.StopServer();
            }
            else
            {
                Logger.DefaultLogger("Server not Started.");
            }
        }
    }
}