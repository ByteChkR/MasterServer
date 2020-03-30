using CommandRunner;

namespace MasterServer.Commands
{
    public class StopServerCommand : AbstractCommand
    {
        public StopServerCommand() :
            base(StopServer, new[] { "--stop-server", "-stop" }, "Stops the Server",
                false)
        {

        }


        private static void StopServer(StartupInfo info, string[] args)
        {
            Program.Master.StopServer();
        }
    }
}