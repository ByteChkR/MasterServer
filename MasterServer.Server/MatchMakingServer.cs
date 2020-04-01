using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Byt3.Serialization;
using MasterServer.Common;
using MasterServer.Common.Networking.Packets;
using MasterServer.Common.Networking.Packets.Serializers;
using MasterServer.Server.ConnectionManaging;
using MasterServer.Server.GameInstanceManaging;
using MasterServer.Server.PortManaging;

namespace MasterServer.Server
{
    public class MatchMakingServer
    {
        private static string CreateArg(string format, params object[] items)
        {
            return string.Format(format, items);
        }
        private string CreateArgs(int port, int noPlayerTimeout, int timeout)
        {
            string noPlayerTimeoutArg1 = CreateArg(Settings.NoPlayerTimeoutArgString, noPlayerTimeout);
            string timeoutArg1 = CreateArg(Settings.TimeoutArgString, timeout);
            string portArg1 = CreateArg(Settings.PortArgString, port);
            string args = Settings.InstanceArguments;
            return $"{args} {noPlayerTimeoutArg1} {timeoutArg1} {portArg1}";
        }

        private readonly MatchMakerSettings Settings;
        private readonly List<WaitingQueueItem> RemoveList;
        private readonly List<WaitingQueueItem> WaitingQueue;
        private readonly PortManager PortManager;
        private readonly GameInstanceManager InstanceManager;
        private readonly ConnectionManager ConnectionManager;
        private bool ForceStop;
        public bool IsRunning { get; private set; }
        public MatchMakingServer(MatchMakerSettings settings)
        {
            Byt3Serializer.AddSerializer<ClientHeartBeatPacket>(new ClientHeartBeatSerializer());
            Byt3Serializer.AddSerializer<ClientHandshakePacket>(new ClientHandshakeSerializer());
            Byt3Serializer.AddSerializer<ClientInstanceReadyPacket>(new ClientInstanceReadySerializer());
            Byt3Serializer.AddSerializer<ServerExitPacket>(new ServerExitSerializer());
            Settings = settings;
            Logger.DefaultLogger(Settings.ToString());
            PortManager = new PortManager(Settings);
            InstanceManager = new GameInstanceManager(Settings);
            ConnectionManager = new ConnectionManager(Settings);
            WaitingQueue = new List<WaitingQueueItem>();
            RemoveList = new List<WaitingQueueItem>();
        }

        public void StartSynchronous()
        {
            ConnectionManager.StartListen();
            Loop();
        }

        public void StartServer()
        {
            IsRunning = true;
            new Thread(Loop).Start();
        }

        public void StopServer()
        {
            ForceStop = true;
        }

        public QueueInfo[] GetQueueInfos()
        {
            QueueInfo[] ret = new QueueInfo[WaitingQueue.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = WaitingQueue[i].GetQueueInfo();
            }

            return ret;
        }

        public GameInstanceInfo[] GetInstanceInfos()
        {
            return InstanceManager.GetInstanceInfos();
        }

        public void StopGameInstances(int[] ports)
        {
            if (ports == null || ports.Length == 0)
                InstanceManager.CloseAllInstances();
            else
            {
                InstanceManager.CloseInstances(ports);
            }
        }

        private void AddToWaitQueue(TcpClient[] clients)
        {
            if (clients.Length == 0) return;
            ClientHandshakePacket chp = new ClientHandshakePacket
            {
                CurrentInstances = InstanceManager.InstanceCount,
                HeartBeat = Settings.HeartBeatInterval,
                MaxInstances = Settings.GameServerPortRange.Range,
                WaitingQueue = WaitingQueue.Count
            };
            WaitingQueue.AddRange(clients.Select(x => new WaitingQueueItem(Settings.HeartBeatInterval, Settings.MaxAllowedMissedHeartBeats, chp, x)));
        }


        private void ProcessWaitQueue()
        {
            for (int i = WaitingQueue.Count - 1; i >= 0; i--)
            {
                if (RemoveList.Contains(WaitingQueue[i]))
                {
                    if(!WaitingQueue[i].IsConnected || WaitingQueue[i].ReceivedEndConnection())
                    RemoveList.Remove(WaitingQueue[i]);
                    WaitingQueue.RemoveAt(i);
                    continue;
                }

                if (!WaitingQueue[i].ReceivedHeartbeat())
                {
                    Logger.DefaultLogger("Client Timed Out: " + WaitingQueue[i].Identifier);
                    WaitingQueue[i].CloseConnection();
                    WaitingQueue.RemoveAt(i);
                }
            }
            WaitingQueue.Sort();
        }

        private void RemoveQueueItem(WaitingQueueItem item)
        {
            Logger.DefaultLogger("Connection Finished: " + item.Identifier);
            //item.CloseConnection();
        }

        private void TryCreateMatch()
        {
            if (Settings.HasFlag(ServerOperationMode.DisableCreateInstance)) return;

            if (WaitingQueue.Count >= Settings.PlayersPerGameInstance && PortManager.HasFreePorts())
            {
                Port port = PortManager.AquirePort();
                InstanceManager.StartInstance(port, CreateArgs(port.PortNum, 20000, 3600000));

                WaitingQueueItem[] players = new WaitingQueueItem[Settings.PlayersPerGameInstance];

                for (int i = 0; i < Settings.PlayersPerGameInstance; i++)
                {
                    players[i] = WaitingQueue[i];
                    RemoveList.Add(WaitingQueue[i]);
                }

                Thread t = new Thread(() =>
                {
                    Thread.Sleep(Settings.PlayersMatchFoundNotifyDelay);
                    for (int i = 0; i < players.Length; i++)
                    {
                        Logger.DefaultLogger("Notifying Player: " + players[i].Identifier);
                        players[i].SendMatchFound(port.PortNum);
                    }

                    Thread.Sleep(1000);

                    for (int i = 0; i < players.Length; i++)
                    {
                        RemoveQueueItem(players[i]);
                    }
                });

                t.Start();
            }
        }



        private void Loop()
        {
            ConnectionManager.StartListen();
            while (!ForceStop)
            {
                ProcessWaitQueue();
                AddToWaitQueue(ConnectionManager.AcceptPendingClients());
                TryCreateMatch();
                Thread.Sleep(Settings.ServerTick);
            }

            ConnectionManager.DisconnectAllClients();
            if (ConnectionManager.IsListening)
                ConnectionManager.StopListen();
            InstanceManager.CloseAllInstances();


            IsRunning = false;
        }
    }
}