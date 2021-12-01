

/// <summary>
/// 客户端发送给服务器的包 接口
/// </summary>
public abstract class CSPacketBase : PacketBase
{
    public override PacketType PacketType
    {
        get
        {
            return PacketType.ClientToServer;
        }
    }
}