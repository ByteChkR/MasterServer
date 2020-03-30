using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MasterServer.Client;
using MasterServer.Common;
using MasterServer.Server;

namespace MasterServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly s = Assembly.GetExecutingAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(s.Location));
            Logger.DefaultLogger("Hello World!");

            if (args.Length == 1 && args[0] == "-client")
                StartClient();
            else if (args.Length == 2 && args[0] == "-client")
            {
                int max = int.Parse(args[1]);
                for (int i = 0; i < max; i++)
                {
                    Start(StartClient);
                }
            }
            else
            {
                StartServer();
                Console.ReadLine();
            }


        }

        private static void StartServer()
        {
            MasterServerSettings ss = MasterServerSettings.Load("./Settings.xml");


            Server.MasterServer master = new Server.MasterServer(ss);
            master.StartServer();

            string read = "";
            while (read != "stop")
            {
                read = Console.ReadLine();
            }

            master.StopServer();

            ss.Save("./Settings.xml");
        }

        private static void StartClient()
        {
            Task<MasterServerAPI.ServerHandshakePacket> handshakeTask = MasterServerAPI.BeginConnectionAsync("213.109.162.193", 19999);
            handshakeTask.Start();
            Logger.DefaultLogger("Waiting for Server");
            while (handshakeTask.Status == TaskStatus.Running)
            {
                Thread.Sleep(100);
                Console.Write(".");
            }

            if (handshakeTask.IsFaulted)
            {
                return;
            }

            MasterServerAPI.ServerHandshakePacket hpack = handshakeTask.Result;
            Logger.DefaultLogger("");

            Logger.DefaultLogger("Server Info:");
            Logger.DefaultLogger($"\tCurrent Game Instances: {hpack.CurrentInstances}/{hpack.MaxInstances}");
            Logger.DefaultLogger($"\tClients in Queue: {hpack.WaitingQueue + 1}");
            Logger.DefaultLogger($"\tHeartbeat: {hpack.HeartBeat}");

            Task<MasterServerAPI.ServerInstanceResultPacket> queueTask = MasterServerAPI.FindMatchAsync(hpack);
            queueTask.Start();

            Logger.DefaultLogger("In Queue..");
            while (queueTask.Status == TaskStatus.Running)
            {
                Thread.Sleep(100);
            }

            Logger.DefaultLogger("Finished Queue.");
            Logger.DefaultLogger($"Game Server Instance Port: {queueTask.Result.Port}");
            Logger.DefaultLogger("Finished Queue.");

        }

        private static Thread Start(ThreadStart ac)
        {
            Thread t = new Thread(ac);
            t.Start();
            return t;
        }
    }
}
