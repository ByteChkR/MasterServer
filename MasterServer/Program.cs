using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Byt3.Serialization;
using CommandRunner;
using MasterServer.Client;
using MasterServer.Commands;
using MasterServer.Common;
using MasterServer.Common.Networking.Packets;
using MasterServer.Common.Networking.Packets.Serializers;
using MasterServer.Server;

namespace MasterServer
{
    internal class Program
    {
        internal static bool Exit;
        internal static bool FirstArgs = true;
        internal static MatchMakingServer MatchMaker;
        internal static MatchMakerSettings Settings = new MatchMakerSettings();
        private static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "--client")
            {
                StartClientSychronous();
                Console.ReadLine();
                return;
            }
            else if (args.Length == 1 && args[0] == "--async-client")
            {
                StartClient();
                Console.ReadLine();
            }
            else
            {
                StartServer(args);
            }

        }

        private static void StartServer(string[] args)
        {
            Assembly s = Assembly.GetExecutingAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(s.Location));



            Runner.AddCommand(new HelpCommand());
            Runner.AddCommand(new LoadSettingsCommand());
            Runner.AddCommand(new ServerOperationModeCommand());
            Runner.AddCommand(new SaveSettingsCommand());
            Runner.AddCommand(new ResetServerCommand());
            Runner.AddCommand(new StartServerCommand());
            Runner.AddCommand(new StopServerCommand());
            Runner.AddCommand(new ExitCLICommand());
            Runner.AddCommand(new KillInstancesCommand());
            Runner.AddCommand(new ListClientQueueCommand());
            Runner.AddCommand(new ListInstancesCommand());

            Logger.DefaultLogger("Console Initialized..");


            Runner.RunCommands(args);
            FirstArgs = false;
            while (!Exit)
            {
                string input = Console.ReadLine();
                if (input == null) continue;
                Runner.RunCommands(input.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            if (MatchMaker != null && MatchMaker.IsRunning) MatchMaker.StopServer();

        }

        private static void StartClientSychronous()
        {
            Byt3Serializer.AddSerializer<ClientHeartBeatPacket>(new ClientHeartBeatSerializer());
            Byt3Serializer.AddSerializer<ClientHandshakePacket>(new ClientHandshakeSerializer());
            Byt3Serializer.AddSerializer<ClientInstanceReadyPacket>(new ClientInstanceReadySerializer());
            Byt3Serializer.AddSerializer<ServerExitPacket>(new ServerExitSerializer());

            MasterServerAPI.ConnectionEvents evs = new MasterServerAPI.ConnectionEvents();
            evs.OnError = (MatchMakingErrorCode e, Exception ex) =>
            {
                Console.WriteLine("Error Code: " + e);
                if (ex != null)
                    throw ex;
            };
            evs.OnStatusUpdate = Console.WriteLine;
            evs.OnSuccess = (MasterServerAPI.ServerInstanceResultPacket packet) =>
            {
                Console.WriteLine("Connection Successful: Error: " + packet.ErrorCode + "  Port:" + packet.Port);
            };

            MasterServerAPI.Queue(evs, "localhost", 19999, new CancellationToken());

            //Task<MasterServerAPI.ServerInstanceResultPacket> queue =
            //    MasterServerAPI.QueueAsync(evs, "localhost", 19999, new CancellationToken());
            //queue.Start();
            //queue.Wait();
            return;
        }

        private static void StartClient()
        {

            Byt3Serializer.AddSerializer<ClientHeartBeatPacket>(new ClientHeartBeatSerializer());
            Byt3Serializer.AddSerializer<ClientHandshakePacket>(new ClientHandshakeSerializer());
            Byt3Serializer.AddSerializer<ClientInstanceReadyPacket>(new ClientInstanceReadySerializer());
            Byt3Serializer.AddSerializer<ServerExitPacket>(new ServerExitSerializer());

            MasterServerAPI.ConnectionEvents evs = new MasterServerAPI.ConnectionEvents();
            evs.OnError = (MatchMakingErrorCode e, Exception ex) =>
            {
                Console.WriteLine("Error Code: " + e);
                if (ex != null)
                    throw ex;
            };
            evs.OnStatusUpdate = Console.WriteLine;
            evs.OnSuccess = (MasterServerAPI.ServerInstanceResultPacket packet) =>
            {
                Console.WriteLine("Connection Successful: Error: " + packet.ErrorCode + "  Port:" + packet.Port);
            };

            Task<MasterServerAPI.ServerInstanceResultPacket> queue =
                MasterServerAPI.QueueAsync(evs, "localhost", 19999, new CancellationToken());
            queue.Start();
            queue.Wait();
            return;

            //    Task<MasterServerAPI.ServerHandshakePacket> handshakeTask = MasterServerAPI.BeginConnectionAsync("localhost", 19999);
            //    handshakeTask.Start();
            //    Logger.DefaultLogger("Waiting for MatchMakingServer");
            //    while (handshakeTask.Status == TaskStatus.Running)
            //    {
            //    }

            //    if (handshakeTask.IsFaulted)
            //    {
            //        throw (handshakeTask.Exception as AggregateException).InnerExceptions[0];
            //    }

            //    MasterServerAPI.ServerHandshakePacket hpack = handshakeTask.Result;
            //    Logger.DefaultLogger("");

            //    Logger.DefaultLogger("MatchMakingServer Info:");
            //    Logger.DefaultLogger($"\tCurrent Game Instances: {hpack.CurrentInstances}/{hpack.MaxInstances}");
            //    Logger.DefaultLogger($"\tClients in Queue: {hpack.WaitingQueue}");
            //    Logger.DefaultLogger($"\tHeartbeat: {hpack.HeartBeat}");

            //    Task<MasterServerAPI.ServerInstanceResultPacket> queueTask = MasterServerAPI.FindMatchAsync(hpack);
            //    queueTask.Start();

            //    Logger.DefaultLogger("In Queue..");
            //    while (queueTask.Status == TaskStatus.Running)
            //    {
            //        Thread.Sleep(100);
            //    }

            //    if (queueTask.IsFaulted)
            //        throw queueTask.Exception;

            //    Logger.DefaultLogger("Finished Queue.");
            //    Logger.DefaultLogger($"Game MatchMakingServer Instance Port: {queueTask.Result.Port}");
            //    Logger.DefaultLogger("Finished Queue.");

        }

        //private static Thread Start(ThreadStart ac)
        //{
        //    Thread t = new Thread(ac);
        //    t.Start();
        //    return t;
        //}
    }
}
