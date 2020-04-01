using System;

namespace MasterServer.Common.Networking.Packets
{
    public class MatchMakingErrorPacket
    {
        public MatchMakingErrorCode ErrorCode;
        public string ErrorMessage;
        public bool IsException;
        public Exception MatchMakingException;
    }
}