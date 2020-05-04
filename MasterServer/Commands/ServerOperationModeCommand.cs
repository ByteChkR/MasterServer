using System;
using Byt3.ADL;
using Byt3.CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer.Commands
{
    public class ServerOperationModeCommand : AbstractCommand
    {
        public ServerOperationModeCommand() :
            base(new[] { "--set-operation-mode", "-setop" }, "Sets the Setup Operation Mode.", false)
        {
            CommandAction = SetServerOperationMode;
        }


        private void SetServerOperationMode(StartupArgumentInfo info, string[] args)
        {
            ServerOperationMode mode =
                args.Length == 0 ? ServerOperationMode.None : Enum.Parse<ServerOperationMode>(args[0], true);

            Logger.Log(LogType.Log, "New Server Mode: " + mode, 1);
            Program.Settings.SetOperationMode(mode);
        }
    }
}