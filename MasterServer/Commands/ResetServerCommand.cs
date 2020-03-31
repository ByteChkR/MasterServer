using System.Threading;
using CommandRunner;
using MasterServer.Common;

namespace MasterServer.Commands
{
    public class ResetServerCommand : AbstractCommand
    {
        public ResetServerCommand() :
            base(Reset, new[] { "--reset-server", "-reset" }, "Stops the MatchMakingServer and Resets it to be brand new.",
                false)
        {

        }


        private static void Reset(StartupInfo info, string[] args)
        {
            if (Program.MatchMaker == null)
            {
                Logger.DefaultLogger("Server not Initialized.");
                return;
            }
            if (Program.MatchMaker.IsRunning)
            {
                Logger.DefaultLogger("Stopping Server...");
                Program.MatchMaker.StopServer();
                Thread.Sleep(1000); //The server has to shut down.
            }

            Program.MatchMaker = null;
        }
    }
}