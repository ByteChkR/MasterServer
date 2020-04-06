using Byt3.CommandRunner;

namespace MasterServer.Commands
{
    public class ExitCLICommand :AbstractCommand
    {
        public ExitCLICommand() :
            base(Exit, new[] { "--exit", "-e", "--quit", "-q" }, "Closes the Command Line",
                false)
        {

        }


        private static void Exit(StartupInfo info, string[] args)
        {
            Program.Exit = true;
        }
    }
}