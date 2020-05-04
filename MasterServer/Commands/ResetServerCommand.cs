using System.Threading;
using Byt3.ADL;
using Byt3.CommandRunner;
using MasterServer.Common;

namespace MasterServer.Commands
{
    public class ResetServerCommand : AbstractCommand
    {
        public ResetServerCommand() :
            base( new[] { "--reset-server", "-reset" }, "Stops the MatchMakingServer and Resets it to be brand new.",
                false)
        {
            CommandAction = Reset;
        }


        private void Reset(StartupArgumentInfo info, string[] args)
        {
            if (Program.MatchMaker == null)
            {
                Logger.Log(LogType.Log, "Server not Initialized.", 1);
                return;
            }
            if (Program.MatchMaker.IsRunning)
            {
                Logger.Log(LogType.Log, "Stopping Server...", 1);
                Program.MatchMaker.StopServer();
                Thread.Sleep(1000); //The server has to shut down.
            }

            Program.MatchMaker = null;
        }
    }
}