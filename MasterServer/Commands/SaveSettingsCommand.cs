using Byt3.ADL;
using Byt3.CommandRunner;
using MasterServer.Common;

namespace MasterServer.Commands
{
    public class SaveSettingsCommand : AbstractCommand
    {
        public SaveSettingsCommand() :
            base( new[] { "--save-settings", "-save" }, "Saves a Settings File", false)
        {
            CommandAction = SaveSettings;
        }


        private void SaveSettings(StartupArgumentInfo info, string[] args)
        {
            if (args.Length > 0)
            {
                Logger.Log(LogType.Log, "Saved Settings.", 1);
                Program.Settings.Save(args[0]);
            }
        }
    }
}