using System;
using CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

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
            SessionInfo[] sinfo = Program.Master.GetSessionInfos();
            string s = $"Client Queue: { Program.Master.QueuedPlayers}\n";
            foreach (SessionInfo sessionInfo in sinfo)
            {
                s += "\tName: " + sessionInfo.Name + "\n";
                s += "\t\tHeartbeats Sent:" + sessionInfo.HeartbeatsSent + "\n";
                s += "\t\tBytes Available:" + sessionInfo.BytesAvailable + "\n\n";
            }

            Logger.DefaultLogger(s);
        }
    }

}