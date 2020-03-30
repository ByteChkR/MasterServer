using CommandRunner;

namespace MasterServer.Commands
{
    public class StartServerCommand :AbstractCommand
    {
        public StartServerCommand() :
            base(StartServer, new[] { "--start-server", "-start" }, "Starts the Server",
                false)
        {

        }


        private static void StartServer(StartupInfo info, string[] args)
        {
            Program.Master.StartServer();
        }
    }
}