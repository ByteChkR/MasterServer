using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MasterServer.Common;
using MasterServer.Common.Networking;
using MasterServer.Common.Networking.Packets;

namespace MasterServer.Client
{
    public static class MasterServerAPI
    {
        public struct ServerInstanceResultPacket
        {
            public int Port;
            public bool Success => ErrorCode == MatchMakingErrorCode.None;
            public MatchMakingErrorCode ErrorCode;
            public Exception ErrorException;
        }

        
        public class ConnectionEvents
        {
            public Action<string> OnStatusUpdate;
            public Action<MatchMakingErrorCode, Exception> OnError;
            public Action<ServerInstanceResultPacket> OnSuccess;
        }

        public struct ServerHandshakePacket
        {
            public TcpClient Client;
            public MatchMakingErrorCode ErrorCode;
            public Exception ErrorException;
            public int MaxInstances;
            public int CurrentInstances;
            public int WaitingQueue;
            public int HeartBeat;
        }


        public static Task<ServerInstanceResultPacket> QueueAsync(ConnectionEvents events, string ip, int port, CancellationToken token)
        {
            return new Task<ServerInstanceResultPacket>(() => Queue(events, ip, port, token));
        }


        public static ServerInstanceResultPacket Queue(ConnectionEvents events, string ip, int port, CancellationToken token)
        {
            ServerHandshakePacket conn = BeginConnection(events, ip, port);
            return FindMatch(events, conn, token);
        }



        //public static Task<ServerHandshakePacket> BeginConnectionAsync(ConnectionEvents events, string ip, int port)
        //{
        //    return new Task<ServerHandshakePacket>(() => BeginConnection(events, ip, port));
        //}


        //public static Task<ServerInstanceResultPacket> FindMatchAsync(ConnectionEvents events, ServerHandshakePacket packet)
        //{
        //    return FindMatchAsync(events, packet, CancellationToken.None);
        //}
        //public static Task<ServerInstanceResultPacket> FindMatchAsync(ConnectionEvents events, string ip, int port)
        //{
        //    return FindMatchAsync(events, ip, port, CancellationToken.None);
        //}

        //public static Task<ServerInstanceResultPacket> FindMatchAsync(ConnectionEvents events, ServerHandshakePacket packet, CancellationToken token)
        //{
        //    return new Task<ServerInstanceResultPacket>(() => FindMatch(events, packet, token));
        //}

        //public static Task<ServerInstanceResultPacket> FindMatchAsync(ConnectionEvents events, string ip, int port, CancellationToken token)
        //{
        //    return new Task<ServerInstanceResultPacket>(() => FindMatch(events, ip, port, token));
        //}


        private static ServerHandshakePacket BeginConnection(ConnectionEvents events, string ip, int port)
        {
            events.OnStatusUpdate?.Invoke("Connecting To Master Server...");
            TcpClient c = null;
            try
            {
                c = new TcpClient(ip, port);
                events.OnStatusUpdate?.Invoke("Connected to Server...");
            }
            catch (Exception connEX)
            {
                events.OnStatusUpdate?.Invoke("Connection Failed... Exception: \n" + connEX.Message);
                events.OnError?.Invoke(MatchMakingErrorCode.SocketException, connEX);
                return new ServerHandshakePacket() { ErrorCode = MatchMakingErrorCode.SocketException, ErrorException = connEX };
            }

            object obj = null;
            try
            {
                events.OnStatusUpdate?.Invoke("Waiting for Handshake...");
                obj = PacketSerializer.Serializer.GetPacket(c.GetStream());
            }
            catch (Exception e)
            {
                events.OnStatusUpdate?.Invoke("Handshake Failed... Exception: \n" + e.Message);
                events.OnError?.Invoke(MatchMakingErrorCode.PacketDeserializationException, e);
                return new ServerHandshakePacket() { ErrorCode = MatchMakingErrorCode.PacketDeserializationException, ErrorException = e };
            }

            if (obj == null)
            {
                events.OnStatusUpdate?.Invoke("Handshake Failed... Deserialized object was null");
                events.OnError?.Invoke(MatchMakingErrorCode.PacketDeserializationException, null);
                return new ServerHandshakePacket() { ErrorCode = MatchMakingErrorCode.PacketDeserializationException };
            }


            if (obj is ClientHandshakePacket)
            {
                events.OnStatusUpdate?.Invoke("Handshake Received..");
                ClientHandshakePacket packet = (ClientHandshakePacket)obj;
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
            events.OnStatusUpdate?.Invoke("Handshake Failed... Wrong Packet Received");
            events.OnError?.Invoke(MatchMakingErrorCode.WrongPacketReceived, null);
            return new ServerHandshakePacket() { ErrorCode = MatchMakingErrorCode.WrongPacketReceived };
            //throw new Exception("Expected Packet of Type ClientHandshakePacket. Got: " + obj.GetType());
        }

        //public static ServerInstanceResultPacket FindMatch(ConnectionEvents events, ServerHandshakePacket packet)
        //{
        //    return FindMatch(events, packet, new CancellationToken());
        //}


        private static ServerInstanceResultPacket FindMatch(ConnectionEvents events, ServerHandshakePacket packet, CancellationToken token)
        {
            if (packet.ErrorCode != MatchMakingErrorCode.None)
            {
                return new ServerInstanceResultPacket() { ErrorCode = packet.ErrorCode, ErrorException = packet.ErrorException };
            }

            
            try
            {
                TcpClient c = packet.Client;

                int waitTime = packet.HeartBeat;
                events.OnStatusUpdate?.Invoke("In Queue..");
                while (c.Connected && c.Available == 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        Logger.DefaultLogger("Aborting Queue");
                        events.OnStatusUpdate?.Invoke("Aborting Queue...");
                        events.OnError?.Invoke(MatchMakingErrorCode.ClientQueueAborted, null);

                        return new ServerInstanceResultPacket() { ErrorCode = MatchMakingErrorCode.ClientQueueAborted };
                    }

                    try
                    {
                        PacketSerializer.Serializer.WritePacket(c.GetStream(), new ClientHeartBeatPacket());
                    }
                    catch (Exception e)
                    {
                        events.OnStatusUpdate?.Invoke("Packet Could not be Deserialized. Exception: " + e.Message);
                        events.OnError?.Invoke(MatchMakingErrorCode.PacketSerializationException, e);
                        return new ServerInstanceResultPacket() { ErrorCode = MatchMakingErrorCode.PacketSerializationException, ErrorException = e };
                    }
                    Thread.Sleep(waitTime);
                    Logger.DefaultLogger("Heartbeat...");
                }



                if (c.Available == 0)
                {
                    events.OnStatusUpdate?.Invoke("Client has Disconnected during Queue.");
                    events.OnError?.Invoke(MatchMakingErrorCode.ClientDisconnectDuringReady, null);
                    return new ServerInstanceResultPacket() { ErrorCode = MatchMakingErrorCode.ClientDisconnectDuringReady };
                }
                ClientInstanceReadyPacket irp;


                events.OnStatusUpdate?.Invoke("Creating Match...");

                try
                {
                    irp = (ClientInstanceReadyPacket)PacketSerializer.Serializer.GetPacket(c.GetStream());

                    events.OnStatusUpdate?.Invoke("Received Packet Data..");
                }
                catch (Exception e)
                {
                    events.OnStatusUpdate?.Invoke("Packet Could not be Deserialized. Exception: " + e.Message);
                    events.OnError?.Invoke(MatchMakingErrorCode.PacketSerializationException, e);
                    return new ServerInstanceResultPacket() { ErrorCode = MatchMakingErrorCode.PacketSerializationException, ErrorException = e };
                }


                events.OnStatusUpdate?.Invoke("Match Starting on Port: " + irp.Port);

                Logger.DefaultLogger($"Client: Current Instances: {packet.CurrentInstances}/{packet.MaxInstances}");
                Logger.DefaultLogger($"Client: Clients in Queue: {packet.WaitingQueue}");
                Logger.DefaultLogger($"Client: Instance Ready: {irp.Port}");
                c.Close();
                ServerInstanceResultPacket ret = new ServerInstanceResultPacket { Port = irp.Port, ErrorCode = MatchMakingErrorCode.None };

                events.OnSuccess.Invoke(ret);
                return ret;
            }
            catch (Exception unhandled)
            {
                events.OnStatusUpdate?.Invoke("Packet Could not be Deserialized. Exception: " + unhandled.Message);
                events.OnError?.Invoke(MatchMakingErrorCode.PacketSerializationException, unhandled);
                return  new ServerInstanceResultPacket(){Port = -1, ErrorCode = MatchMakingErrorCode.UnhandledError, ErrorException = unhandled };
            }
            
        }

        //public static ServerInstanceResultPacket FindMatch(ConnectionEvents events, string ip, int port, CancellationToken token)
        //{
        //    return FindMatch(events, BeginConnection(events, ip, port), token);
        //}
    }
}
