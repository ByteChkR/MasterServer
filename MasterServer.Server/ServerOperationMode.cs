using System;

namespace MasterServer.Server
{
    [Flags]
    public enum ServerOperationMode
    {
        None = 0,
        DisableTcpAccept = 1,
        DisableCreateInstance = 2,
        DisableServer = 3
    }
}