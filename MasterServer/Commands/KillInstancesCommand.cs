using Byt3.CommandRunner;
using MasterServer.Server.GameInstanceManaging;

namespace MasterServer.Commands
{
    public class KillInstancesCommand : AbstractCommand
    {
        public KillInstancesCommand() :
            base(KillInstances, new[] { "--kill-server-instances", "-ks" }, "Kills all or specified server instances.",
                false)
        {

        }


        private static void KillInstances(StartupInfo info, string[] args)
        {
            int[] ports = new int[args.Length];
            if (args.Length != 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    ports[i] = int.Parse(args[i]);
                }
            }
            else
            {
                GameInstanceInfo[] sis = Program.MatchMaker.GetInstanceInfos();
                ports = new int[sis.Length];
                for (int i = 0; i < sis.Length; i++)
                {
                    ports[i] = sis[i].Port;
                }
            }
            Program.MatchMaker.StopGameInstances(ports);
        }
    }
}