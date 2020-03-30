namespace MasterServer.Server
{
    public struct PortRange
    {
        public int Min;
        public int Max;
        public int Range=>Max-Min;

        public override string ToString()
        {
            return $"Min: {Min} Max: {Max}";
        }
    }
}