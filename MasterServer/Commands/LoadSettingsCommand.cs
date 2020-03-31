using CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer.Commands
{
    public class LoadSettingsCommand : AbstractCommand
    {
        public LoadSettingsCommand() :
            base(LoadSettings, new[] { "--load-settings", "-load" }, "Loads a Settings File", false)
        {

        }


        private static void LoadSettings(StartupInfo info, string[] args)
        {
            if (args.Length != 0)
            {
                Program.Settings = MatchMakerSettings.Load(args[0]);
                Logger.DefaultLogger("Loaded Settings.");
            }
            else
            {
                Program.Settings= new MatchMakerSettings();
                Logger.DefaultLogger("Settings set to Default.");
            }
        }
    }
}