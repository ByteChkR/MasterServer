using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using MasterServer.Common;

namespace MasterServer.Server
{
    public class MasterServer
    {
        internal MasterServerSettings Settings;
        internal InstanceManager InstanceManager;
        private TcpListener Listener;

        private List<ClientSession> ActiveSessions = new List<ClientSession>();

        public int QueuedPlayers => ActiveSessions.Count;

        private bool ForceStop;

        private object ActiveSessionsLockObj = new object();
        private object ForceStopLockObj = new object();

        public MasterServer(MasterServerSettings settings)
        {
            Logger.DefaultLogger("Initializing Master Server...");
            Settings = settings;
            Logger.DefaultLogger(Settings.ToString());
            InstanceManager = new InstanceManager(this);
            Listener = new TcpListener(IPAddress.Any, Settings.MasterPort);
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            InstanceManager.CloseAll();
            lock (ActiveSessionsLockObj)
                foreach (ClientSession activeSession in ActiveSessions)
                {
                    activeSession.Client.Close();
                }
        }

        public void StartServer()
        {
            Logger.DefaultLogger("Starting Master Server...");
            Thread listener = new Thread(ListenerServerLoop);
            listener.Start();
            Thread instance = new Thread(InstanceServerLoop);
            instance.Start();
            //InstanceServerLoop();
        }

        public void StopServer()
        {
            Logger.DefaultLogger("Stopping Master Server...");
            lock (ForceStopLockObj) ForceStop = true;
        }

        private List<ClientSession> GetReadyPlayers()
        {
            lock (ActiveSessionsLockObj)
            {
                return ActiveSessions.Where(x => x.State == ClientSession.ClientSessionState.Queued).ToList();
            }
        }

        private void RemoveInactiveSessions()
        {
            lock (ActiveSessionsLockObj)
            {
                for (int i = ActiveSessions.Count - 1; i >= 0; i--)
                {
                    if (ActiveSessions[i].State == ClientSession.ClientSessionState.Closed ||
                        ActiveSessions[i].State == ClientSession.ClientSessionState.Invalid)
                    {
                        IPEndPoint ep = ((IPEndPoint)ActiveSessions[i].Client.Client.RemoteEndPoint);
                        string ip = ep.Address + ":" + ep.Port;
                        Logger.DefaultLogger($"Removing Session {ip} with State: " + ActiveSessions[i].State);
                        ActiveSessions.RemoveAt(i);
                    }
                }
            }
        }

        private void InstanceServerLoop()
        {
            while (!ForceStop)
            {
                RemoveInactiveSessions();
                InstanceManager.RemoveInactiveGameInstances();
                if (InstanceManager.CanCreateGame())
                {
                    List<ClientSession> queued = GetReadyPlayers();
                    if (queued.Count >= 2)
                    {
                        queued.Sort();
                        List<ClientSession> match = queued.Take(2).ToList();

                        match.ForEach(x => x.SetJoining());

                        InstanceManager.CreateGame(match.ToArray());
                    }
                }
                Thread.Sleep(Settings.ServerInstanceTick);
            }

            foreach (ClientSession activeSession in ActiveSessions)
            {
                activeSession.StopSession();
            }

            ActiveSessions.Clear();
        }

        private void ListenerServerLoop()
        {
            Listener.Start();

            while (!ForceStop)
            {
                if(Listener.Pending())
                {
                    TcpClient client = Listener.AcceptTcpClient();
                    Logger.DefaultLogger("Accepting Connection...");

                    StartSession(client);
                }
                Thread.Sleep(Settings.ServerListenerTick);
            }

            Listener.Stop();
        }

        private void StartSession(TcpClient client)
        {
            IPEndPoint ep = ((IPEndPoint)client.Client.RemoteEndPoint);
            string ip = ep.Address + ":" + ep.Port;
            Logger.DefaultLogger("Starting Client Session: " + ip);

            ClientSession session = new ClientSession(this, client);
            AddActiveSession(session);

            Thread t = new Thread(session.StartSession);
            t.Start();


        }

        public void AddActiveSession(ClientSession session)
        {
            lock (ActiveSessionsLockObj)
            {
                ActiveSessions.Add(session);
            }
        }


    }
}