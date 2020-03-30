using System.IO;
using System.Xml.Serialization;

namespace MasterServer.Server
{
    public struct MasterServerSettings
    {
        public int MasterPort;
        public PortRange GameServerPortRange;
        public int MaxGameServers => GameServerPortRange.Range;
        public int ServerListenerTick;
        public int ServerInstanceTick;
        public int HeartbeatTimeout;
        public int MinTimeHeartBeat;
        public string GameServerPath;

        public override string ToString()
        {
            return
                $"Master Server Settings: \nMaster Port: {MasterPort}\nGame Server Port Range: {GameServerPortRange}\nMaxGameServers: {MaxGameServers}\nServer Tick: {ServerListenerTick}\nHeartbeat Timeout: {HeartbeatTimeout}\nHeartbeat Time: {MinTimeHeartBeat}\nGame Server Path: {GameServerPath}";
        }

        public void Save(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            XmlSerializer xs = new XmlSerializer(typeof(MasterServerSettings));
            Stream s = File.OpenWrite(path);
            xs.Serialize(s, this);
            s.Close();
        }

        public static MasterServerSettings Load(string path)
        {
            if (File.Exists(path))
            {
                XmlSerializer xs = new XmlSerializer(typeof(MasterServerSettings));
                Stream s = File.OpenRead(path);
                MasterServerSettings ret = (MasterServerSettings)xs.Deserialize(s);
                s.Close();
                return ret;
            }

            MasterServerSettings ss = new MasterServerSettings();
            ss.GameServerPortRange = new PortRange { Min = 20000, Max = 20010 };
            ss.MasterPort = 19999;
            ss.HeartbeatTimeout = 1000;
            ss.MinTimeHeartBeat = 100;
            ss.ServerListenerTick = 100;
            string p = Path.GetFullPath("./Game/WindowsStandaloneHeadless.exe");
            ss.GameServerPath = p;

            return ss;
        }

    }
}