using Byt3.ADL;
using Byt3.CommandRunner;
using MasterServer.Common;

namespace MasterServer.Commands
{
    public class StopServerCommand : AbstractCommand
    {
        public StopServerCommand() :
            base(new[] { "--stop-server", "-stop" }, "Stops the MatchMakingServer",
                false)
        {
            CommandAction = StopServer;
        }


        private void StopServer(StartupArgumentInfo info, string[] args)
        {
            if(Program.MatchMaker.IsRunning)
            {
                Logger.Log(LogType.Log, "Stopping Server...");
                Program.MatchMaker.StopServer();
            }
            else
            {
                Logger.Log(LogType.Log, "Server not Started.");
            }
        }
    }
}