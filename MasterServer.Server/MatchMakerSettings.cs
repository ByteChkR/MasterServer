using System;
using System.IO;
using System.Xml.Serialization;
using MasterServer.Common;
using MasterServer.Server.PortManaging;

namespace MasterServer.Server
{
    public class MatchMakerSettings
    {
        [XmlIgnore]
        internal ServerOperationMode OperationMode;
        public int MaxGameServers => GameServerPortRange.Range;
        public int MasterPort;
        public PortRange GameServerPortRange;
        public int MaxPlayersInQueue;
        public int ServerTick;
        public int MaxAllowedMissedHeartBeats;
        public int HeartBeatInterval;
        public int PlayersPerGameInstance;
        public int PlayersMatchFoundNotifyDelay;
        public bool InstanceUseShellExecute;
        public bool InstanceCreateNoWindow;
        public string GameServerPath;
        public string InstanceArguments;
        public string PortArgString;
        public string TimeoutArgString;
        public string NoPlayerTimeoutArgString;

        

        public MatchMakerSettings()
        {
            MakeDefault(this);
        }

        public void SetOperationMode(ServerOperationMode mode)
        {
            OperationMode = mode;
        }

        internal bool HasFlag(ServerOperationMode mode)
        {
            return (OperationMode & mode) != 0;
        }

        public override string ToString()
        {
            return
                $"MatchMakingServer Settings:\r\n\tMatchMaker Port: {MasterPort}\r\n\tPlayers Match Found Notify Delay(MS): {PlayersMatchFoundNotifyDelay}\r\n\tServer Tick (MS): {ServerTick}\r\n\tHeartBeat Interval (MS): {HeartBeatInterval}\r\n\tMax Allowed Missed HeartBeats: {MaxAllowedMissedHeartBeats}\r\n\tGame Server Settings:\r\n\t\tGameServer Ports: {GameServerPortRange}\r\n\t\tMax Game Servers: {MaxGameServers}\r\n\t\tPlayers Per Game Instance: {PlayersPerGameInstance}\r\n\t\tInstance Use Shell Execute: {InstanceUseShellExecute}\r\n\t\tInstance Create No Window: {InstanceCreateNoWindow}\r\n\t\tGame Server Path: {GameServerPath}\r\n\t\tPort Format String: {PortArgString}\r\n\t\tTimeout Format String: {TimeoutArgString}\r\n\t\tNo Player Timeout Format String: {NoPlayerTimeoutArgString}\r\n\t\tInstance Arguments: {InstanceArguments}";
        }

        public void Save(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            XmlSerializer xs = new XmlSerializer(typeof(MatchMakerSettings));
            Stream s = File.OpenWrite(path);
            xs.Serialize(s, this);
            s.Close();
        }

        public static MatchMakerSettings Load(string path)
        {
            Stream s = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(MatchMakerSettings));
                s = File.OpenRead(path);
                MatchMakerSettings ret = (MatchMakerSettings)xs.Deserialize(s);
                s.Close();
                return ret;
            }
            catch (Exception )
            {
                s?.Close();
                Logger.DefaultLogger("Could Not load the Settings File at location " + path);
            }

            MatchMakerSettings ss = new MatchMakerSettings();
            

            return MakeDefault(ss);
        }

        private static MatchMakerSettings MakeDefault(MatchMakerSettings ss)
        {

            //string closeOnMatchEndedArg = "Game.HeadlessInfo.CloseOnMatchEnded:true";
            //string noPlayerTimeoutArg = $"Game.HeadlessInfo.NoPlayerTimeout:{noPlayerTimeout}";
            //string timeoutArg = $"Game.HeadlessInfo.Timeout:{timeout}";
            //string portArg = $"Game.Network.DefaultAddress.Port:{port}";

            //return $"{closeOnMatchEndedArg} {noPlayerTimeoutArg} {timeoutArg} {portArg}";

            ss.InstanceArguments = "Game.HeadlessInfo.CloseOnMatchEnded:true";
            ss.PortArgString = "Game.Network.DefaultAddress.Port:{0}";
            ss.NoPlayerTimeoutArgString = "Game.HeadlessInfo.NoPlayerTimeout:{0}";
            ss.TimeoutArgString = "Game.HeadlessInfo.Timeout:{0}";
            ss.InstanceCreateNoWindow = true;
            ss.InstanceUseShellExecute = false;
            ss.GameServerPortRange = new PortRange(20000, 20010);
            ss.MasterPort = 19999;
            ss.HeartBeatInterval = 1000;
            ss.MaxAllowedMissedHeartBeats = 2;
            ss.ServerTick = 100;
            ss.PlayersPerGameInstance = 2;
            ss.MaxPlayersInQueue = 10;
            ss.PlayersMatchFoundNotifyDelay = 1000;
            string p = Path.GetFullPath("./Game/WindowsStandaloneHeadless.exe");
            ss.GameServerPath = p;
            return ss;
        }

    }
}