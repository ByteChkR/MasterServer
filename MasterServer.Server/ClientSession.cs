using System;
using System.Net.Sockets;
using System.Threading;
using MasterServer.Common;
using MasterServer.Common.Packets;

namespace MasterServer.Server
{
    public class ClientSession : IComparable, IComparable<ClientSession>
    {
        public enum ClientSessionState
        {
            /// <summary>
            /// Between Constructor Call and end of handshake
            /// </summary>
            Initializing,
            /// <summary>
            /// After handshake
            /// </summary>
            Queued,
            /// <summary>
            /// between match found and client session closing after sending the port of the started instance.
            /// </summary>
            Joining,
            /// <summary>
            /// Session was finished with success or because of a server shut down
            /// </summary>
            Closed,
            /// <summary>
            /// When the Client disconnects
            /// </summary>
            Invalid
        }

        public ClientSessionState State { get; private set; }

        //Ready Info
        private bool InstanceReady;
        private int Port;

        //Initialized in constructor
        public TcpClient Client;
        private MasterServer Master;

        //Gets initialized on StartSession
        private Thread SessionThread;


        //Stop Flag
        private bool ForceStop;
        private DateTime TimeAdded;


        private object InstanceReadyLockObj = new object();
        private object StateLockObj = new object();
        private object ForceStopLockObj = new object();


        public ClientSession(MasterServer master, TcpClient client)
        {
            TimeAdded = DateTime.UtcNow;
            State = ClientSessionState.Initializing;
            Master = master;
            Client = client;
        }

        public void SetJoining()
        {
            lock (StateLockObj)
            {
                if (State == ClientSessionState.Queued)
                    State = ClientSessionState.Joining;
            }
        }

        public void SetInstanceReady(bool state, int port)
        {
            Logger.DefaultLogger("Client Session was Signaled that a Game Instance is ready...");
            lock (InstanceReadyLockObj)
            {
                InstanceReady = state;
                Port = port;
            }
        }

        public void StartSession()
        {
            Logger.DefaultLogger("Starting the Client Session...");
            SessionThread = Thread.CurrentThread;
            InitHandshake();
            lock (StateLockObj)
                State = ClientSessionState.Queued;
            SessionLoop();
        }

        public void StopSession()
        {
            Logger.DefaultLogger("Stopping the Client Session...");
            lock (ForceStopLockObj)
            {
                ForceStop = true;
            }
        }

        private void InitHandshake()
        {
            ClientHandshakePacket handshake = new ClientHandshakePacket { CurrentInstances = Master.InstanceManager.InstanceCount, WaitingQueue = Master.QueuedPlayers, HeartBeat = Master.Settings.MinTimeHeartBeat, MaxInstances = Master.Settings.MaxGameServers };
            Client.SendPacket(handshake);
        }


        private void SessionLoop()
        {
            int id = 0;
            bool invalid = false;
            while (!InstanceReady)
            {
                invalid = true;
                //Client.SendPacket(new ClientHeartBeatPacket() { ID = ++id });
                Thread.Sleep(Master.Settings.MinTimeHeartBeat);
                int timePassed = Master.Settings.MinTimeHeartBeat;
                while (timePassed < Master.Settings.HeartbeatTimeout)
                {
                    if (Client.Available > 4)
                    {
                        int packets = Client.Available / 4;
                        ClientHeartBeatPacket packet = new ClientHeartBeatPacket();
                        for (int i = 0; i < packets; i++)
                        {
                            Client.ReceivePacket(ref packet);
                        }
                        invalid = false;
                    }
                    Thread.Sleep(10);
                    timePassed += 10;
                }

                if (invalid || ForceStop) break;
            }

            if (invalid)
            {

                lock (StateLockObj)
                    State = ClientSessionState.Invalid;
                Logger.DefaultLogger("Client Timed Out");
                //Do Stuff

                return; //Exit Client Session Thread
            }

            if (ForceStop)
            {
                lock (StateLockObj)
                    State = ClientSessionState.Closed;

                Logger.DefaultLogger("Server Closed");
                return;
            }

            Client.SendPacket(new ClientInstanceReadyPacket() { Port = Port });
            lock (StateLockObj)
                State = ClientSessionState.Closed;
        }

        public int CompareTo(ClientSession session)
        {
            return TimeAdded.CompareTo(session.TimeAdded);
        }


        public int CompareTo(object obj)
        {
            if (obj is ClientSession cs)
                return CompareTo(cs);
            return -1;
        }
    }
}