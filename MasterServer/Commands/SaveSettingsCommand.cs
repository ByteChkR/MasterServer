using CommandRunner;
using MasterServer.Common;

namespace MasterServer.Commands
{
    public class SaveSettingsCommand : AbstractCommand
    {
        public SaveSettingsCommand() :
            base(SaveSettings, new[] { "--save-settings", "-save" }, "Saves a Settings File", false)
        {

        }


        private static void SaveSettings(StartupInfo info, string[] args)
        {
            if (args.Length > 0)
            {
                Logger.DefaultLogger("Saved Settings.");
                Program.Settings.Save(args[0]);
            }
        }
    }
}