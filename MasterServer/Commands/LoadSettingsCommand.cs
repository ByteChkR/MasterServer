using Byt3.ADL;
using Byt3.CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer.Commands
{
    public class LoadSettingsCommand : AbstractCommand
    {
        public LoadSettingsCommand() :
            base( new[] { "--load-settings", "-load" }, "Loads a Settings File", false)
        {
            CommandAction = LoadSettings;
        }


        private void LoadSettings(StartupArgumentInfo info, string[] args)
        {
            if (args.Length != 0)
            {
                Program.Settings = MatchMakerSettings.Load(args[0]);
                Logger.Log(LogType.Log, "Loaded Settings.");
            }
            else
            {
                Program.Settings= new MatchMakerSettings();
                Logger.Log(LogType.Log, "Settings set to Default.");
            }
        }
    }
}