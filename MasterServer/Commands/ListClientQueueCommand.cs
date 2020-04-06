using Byt3.ADL;
using Byt3.CommandRunner;
using MasterServer.Common;
using MasterServer.Server.ConnectionManaging;

namespace MasterServer.Commands
{
    public class ListClientQueueCommand : AbstractCommand
    {


        public ListClientQueueCommand() :
            base(new[] { "--list-queued-clients", "-lq" }, "Lists all Clients in Queue",
            false)
        {
            CommandAction = ListClientQueueCount;
        }


        private void ListClientQueueCount(StartupArgumentInfo info, string[] args)
        {
            QueueInfo[] sinfo = Program.MatchMaker.GetQueueInfos();
            string s = $"Client Queue: { sinfo.Length}\n";
            foreach (QueueInfo sessionInfo in sinfo)
            {
                s += "\tName: " + sessionInfo.Name + "\n";
                s += "\t\tTime In Queue:" + sessionInfo.TimeInQueue + "\n";
                s += "\t\tHeartbeats Sent:" + sessionInfo.HeartbeatsSent + "\n\n";
            }

            Logger.Log(LogType.Log, s);
        }
    }

}