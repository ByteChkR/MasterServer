using CommandRunner;
using MasterServer.Common;

namespace MasterServer.Commands
{
    public class HelpCommand : AbstractCommand
    {
        public HelpCommand() :
            base( Help, new[] { "--help", "-h", "--?", "-?" }, "Closes the Command Line",
                true)
        {

        }


        private static void Help(StartupInfo info, string[] args)
        {
            if (Program.FirstArgs && info.CommandCount == 0) return;
            for (int i = 0; i < Runner.CommandCount; i++)
            {
                Logger.DefaultLogger(Runner.GetCommandAt(i).ToString());
            }
        }
    }
}