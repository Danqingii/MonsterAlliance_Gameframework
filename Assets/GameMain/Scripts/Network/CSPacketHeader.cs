namespace Game
{
    /// <summary>
    /// 客户端发送给服务器的 包头
    /// </summary>
    public sealed class CSPacketHeader : PacketHeaderBase
    {
        public override PacketType PacketType
        {
            get
            {
                return PacketType.ClientToServer;
            }
        }
    }
}