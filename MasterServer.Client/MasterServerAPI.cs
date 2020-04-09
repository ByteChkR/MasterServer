using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Byt3.Serialization;
using MasterServer.Common;
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
            public override string ToString()
            {
                return $"Connection Events: Update: {OnStatusUpdate != null} Error: {OnError != null} Success: {OnSuccess != null}";
            }
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

            public override string ToString()
            {
                EndPoint epl = Client.Client.LocalEndPoint;
                EndPoint epr = Client.Client.RemoteEndPoint;
                return
                    $"Handshake:\r\n\tCurrent Game Instances: {CurrentInstances}/{MaxInstances}\r\n\tClients in Queue: {WaitingQueue}\r\n\tHeartbeat: {HeartBeat}\r\n\tError Code: {ErrorCode}\r\n\tException: {ErrorException}\r\n\tClient Local: {epl}\r\n\tClient Remote: {epr}";
            }
        }


        public static Task<ServerInstanceResultPacket> QueueAsync(ConnectionEvents events, string ip, int port, CancellationToken token)
        {
            Logger.DefaultLogger(events.ToString());
            return new Task<ServerInstanceResultPacket>(() => Queue(events, ip, port, token));
        }


        public static ServerInstanceResultPacket Queue(ConnectionEvents events, string ip, int port, CancellationToken token)
        {
            ServerHandshakePacket conn = BeginConnection(events, ip, port);
            return FindMatch(events, conn, token);
        }

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
                events.OnError?.Invoke(MatchMakingErrorCode.SocketException, connEX);
                events.OnStatusUpdate?.Invoke("Connection Failed... Exception: \n" + connEX.Message);
                return new ServerHandshakePacket() { ErrorCode = MatchMakingErrorCode.SocketException, ErrorException = connEX };
            }

            object obj = null;
            try
            {
                events.OnStatusUpdate?.Invoke("Waiting for Handshake...");
                if (!Byt3Serializer.TryReadPacket(c.GetStream(), out obj))
                {
                    throw new Exception();
                }
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


            //try
            //{
            TcpClient c = packet.Client;

            int waitTime = packet.HeartBeat;
            events.OnStatusUpdate?.Invoke("In Queue..");
            Logger.DefaultLogger(packet.ToString());
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
                    Byt3Serializer.WritePacket(c.GetStream(), new ClientHeartBeatPacket());
                }
                catch (Exception e)
                {
                    events.OnStatusUpdate?.Invoke("Packet Could not be Deserialized. Exception: " + e.Message);

                    break; //We could still have some data in the stream that could be our instance ready packet

                   //events.OnError?.Invoke(MatchMakingErrorCode.PacketSerializationException, e);
                    //return new ServerInstanceResultPacket() { ErrorCode = MatchMakingErrorCode.PacketSerializationException, ErrorException = e };
                }
                Thread.Sleep(waitTime);
                Logger.DefaultLogger("Heartbeat...");
            }

            

            if (c.Available == 0)
            {
                events.OnStatusUpdate?.Invoke("Client or Server has Disconnected during Queue.");
                events.OnError?.Invoke(MatchMakingErrorCode.ClientDisconnectDuringReady, null);
                return new ServerInstanceResultPacket() { ErrorCode = MatchMakingErrorCode.ClientDisconnectDuringReady };
            }
            ClientInstanceReadyPacket irp;


            events.OnStatusUpdate?.Invoke("Creating Match...");

            try
            {
                if (!Byt3Serializer.TryReadPacket(c.GetStream(), out irp))
                {
                    throw new Exception();
                }

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

            if (c.Connected)
            {
                c.Close();
            }


            ServerInstanceResultPacket ret = new ServerInstanceResultPacket { Port = irp.Port, ErrorCode = MatchMakingErrorCode.None };

            events.OnSuccess?.Invoke(ret);
            return ret;
            //}
            //catch (Exception unhandled)
            //{
            //    events.OnStatusUpdate?.Invoke("Packet Could not be Deserialized. Exception: " + unhandled.Message);
            //    events.OnError?.Invoke(MatchMakingErrorCode.PacketSerializationException, unhandled);
            //    return new ServerInstanceResultPacket() { Port = -1, ErrorCode = MatchMakingErrorCode.UnhandledError, ErrorException = unhandled };
            //}

        }

        //public static ServerInstanceResultPacket FindMatch(ConnectionEvents events, string ip, int port, CancellationToken token)
        //{
        //    return FindMatch(events, BeginConnection(events, ip, port), token);
        //}
    }
}
