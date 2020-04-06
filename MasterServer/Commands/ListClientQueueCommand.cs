using Byt3.CommandRunner;
using MasterServer.Common;
using MasterServer.Server.ConnectionManaging;

namespace MasterServer.Commands
{
    public class ListClientQueueCommand : AbstractCommand
    {


        public ListClientQueueCommand() :
            base(ListClientQueueCount, new[] { "--list-queued-clients", "-lq" }, "Lists all Clients in Queue",
            false)
        {

        }


        private static void ListClientQueueCount(StartupInfo info, string[] args)
        {
            QueueInfo[] sinfo = Program.MatchMaker.GetQueueInfos();
            string s = $"Client Queue: { sinfo.Length}\n";
            foreach (QueueInfo sessionInfo in sinfo)
            {
                s += "\tName: " + sessionInfo.Name + "\n";
                s += "\t\tTime In Queue:" + sessionInfo.TimeInQueue + "\n";
                s += "\t\tHeartbeats Sent:" + sessionInfo.HeartbeatsSent + "\n\n";
            }

            Logger.DefaultLogger(s);
        }
    }

}