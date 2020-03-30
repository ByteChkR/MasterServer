using CommandRunner;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer.Commands
{
    public class KillIInstancesCommand : AbstractCommand
    {
        public KillIInstancesCommand() :
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
                ServerInstanceInfo[] sis = Program.Master.GetServerInstanceInfos();
                ports = new int[sis.Length];
                for (int i = 0; i < sis.Length; i++)
                {
                    ports[i] = sis[i].Port;
                }
            }
            Program.Master.StopGameInstances(ports);
        }
    }
}