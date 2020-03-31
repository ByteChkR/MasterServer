using System;
using CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer.Commands
{
    public class ServerOperationModeCommand : AbstractCommand
    {
        public ServerOperationModeCommand() :
            base(SetServerOperationMode, new[] { "--set-operation-mode", "-setop" }, "Sets the Setup Operation Mode.", false)
        {

        }


        private static void SetServerOperationMode(StartupInfo info, string[] args)
        {
            ServerOperationMode mode =
                args.Length == 0 ? ServerOperationMode.None : Enum.Parse<ServerOperationMode>(args[0], true);

            Logger.DefaultLogger("New Server Mode: " + mode);
            Program.Settings.SetOperationMode(mode);
        }
    }
}