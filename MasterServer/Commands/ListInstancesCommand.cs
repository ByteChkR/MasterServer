using Byt3.CommandRunner;
using MasterServer.Common;
using MasterServer.Server.GameInstanceManaging;

namespace MasterServer.Commands
{
    public class ListInstancesCommand:AbstractCommand
    {
        public ListInstancesCommand() :
            base(ListInstances, new[] { "--list-server-instances", "-ls" }, "Lists all Running Game Servers",
                false)
        {

        }


        private static void ListInstances(StartupInfo info, string[] args)
        {
            GameInstanceInfo[] sinfos = Program.MatchMaker.GetInstanceInfos();
            string s = $"Instances: {sinfos.Length}\n";
            foreach (GameInstanceInfo serverInstanceInfo in sinfos)
            {
                s += "\tPort: " + serverInstanceInfo.Port + "\n";
                s += "\t\tName: " + serverInstanceInfo.Name + "\n";
                s += "\t\tUptime: " + serverInstanceInfo.UpTime + "\n\n";
            }

            Logger.DefaultLogger(s);
        }
    }
}