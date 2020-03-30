using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MasterServer.Common;
using MasterServer.Common.Packets;

namespace MasterServer.Client
{
    public static class MasterServerAPI
    {
        public struct ServerInstanceResultPacket
        {
            public int Port;
            public bool Success;
        }

        public struct ServerHandshakePacket
        {
            public TcpClient Client;
            public int MaxInstances;
            public int CurrentInstances;
            public int WaitingQueue;
            public int HeartBeat;
        }

        public static Task<ServerHandshakePacket> BeginConnectionAsync(string ip, int port)
        {
            return new Task<ServerHandshakePacket>(() => BeginConnection(ip, port));
        }


        public static Task<ServerInstanceResultPacket> FindMatchAsync(ServerHandshakePacket packet)
        {
            return FindMatchAsync(packet, CancellationToken.None);
        }
        public static Task<ServerInstanceResultPacket> FindMatchAsync(string ip, int port)
        {
            return FindMatchAsync(ip, port, CancellationToken.None);
        }

        public static Task<ServerInstanceResultPacket> FindMatchAsync(ServerHandshakePacket packet, CancellationToken token)
        {
            return new Task<ServerInstanceResultPacket>(() => FindMatch(packet, token));
        }
        
        public static Task<ServerInstanceResultPacket> FindMatchAsync(string ip, int port, CancellationToken token)
        {
            return new Task<ServerInstanceResultPacket>(() => FindMatch(ip, port, token));
        }
        

        public static ServerHandshakePacket BeginConnection(string ip, int port)
        {
            TcpClient c = new TcpClient(ip, port);
            ClientHandshakePacket packet = new ClientHandshakePacket();
            c.ReceivePacket(ref packet);
            ServerHandshakePacket sp = new ServerHandshakePacket
            {
                Client = c,
                HeartBeat = packet.HeartBeat,
                CurrentInstances = packet.CurrentInstances,
                MaxInstances = packet.MaxInstances,
                WaitingQueue = packet.WaitingQueue
            };
            return sp;
        }



        public static ServerInstanceResultPacket FindMatch(ServerHandshakePacket packet, CancellationToken token)
        {
            TcpClient c = packet.Client;

            int waitTime = packet.HeartBeat;
            while (c.Available == 0)
            {
                if (token.IsCancellationRequested)
                {
                    return new ServerInstanceResultPacket();
                }
                Thread.Sleep(waitTime);
                c.SendPacket(new ClientHeartBeatPacket());
                Logger.DefaultLogger("Heartbeat...");
            }
            ClientInstanceReadyPacket irp = new ClientInstanceReadyPacket();
            c.ReceivePacket(ref irp);

            Logger.DefaultLogger($"Client: Current Instances: {packet.CurrentInstances}/{packet.MaxInstances}");
            Logger.DefaultLogger($"Client: Clients in Queue: {packet.WaitingQueue}");
            Logger.DefaultLogger($"Client: Instance Ready: {irp.Port}");
            c.Close();
            ServerInstanceResultPacket ret = new ServerInstanceResultPacket { Port = irp.Port, Success = true };
            return ret;
        }

        public static ServerInstanceResultPacket FindMatch(string ip, int port, CancellationToken token)
        {
            return FindMatch(BeginConnection(ip, port), token);
        }
    }
}
