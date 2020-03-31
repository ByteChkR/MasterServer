using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MasterServer.Common;

namespace MasterServer.Server.ConnectionManaging
{
    public class ConnectionManager
    {
        private readonly Queue<TcpClient> InitializationQueue;
        private TcpListener Listener;

        public bool IsListening { get; private set; }
        public bool HasPendingClients => !Settings.HasFlag(ServerOperationMode.DisableTcpAccept) || Listener.Pending();
        public int AcceptedQueueCount => InitializationQueue.Count;
        private MatchMakerSettings Settings;

        public ConnectionManager(MatchMakerSettings settings)
        {
            Settings = settings;
            Logger.DefaultLogger("Initializing Connection Manager on Port: " + Settings.MasterPort);
            InitializationQueue = new Queue<TcpClient>();
            Listener = new TcpListener(IPAddress.Any, Settings.MasterPort);
        }

        public void StartListen()
        {
            IsListening = true;
            Listener.Start();
        }

        public void StopListen()
        {
            IsListening = false;
            Listener.Stop();
        }

        public void DisconnectAllClients()
        {
            while (InitializationQueue.Count != 0)
            {
                TcpClient c = InitializationQueue.Dequeue();
                c.Close();
            }

            while (Listener.Pending())
            {
                TcpClient c = Listener.AcceptTcpClient();
                c.Close();
            }
        }

        public TcpClient[] AcceptPendingClients()
        {
            if (Settings.HasFlag(ServerOperationMode.DisableTcpAccept)) return new TcpClient[0];
            List<TcpClient> clients = new List<TcpClient>();
            while (Listener.Pending() && clients.Count < Settings.MaxPlayersInQueue)
            {
                clients.Add(Listener.AcceptTcpClient());
            }

            return clients.ToArray();
        }

        public TcpClient GetNextClient()
        {
            if (InitializationQueue.Count == 0) throw new InvalidOperationException("No Client in Initialization Queue");
            return InitializationQueue.Dequeue();
        }


    }
}