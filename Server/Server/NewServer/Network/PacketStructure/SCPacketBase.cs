

/// <summary>
/// 服务器发给客户端的包 接口
/// </summary>
public abstract class SCPacketBase : PacketBase
{
    public override PacketType PacketType
    {
        get
        {
            return PacketType.ServerToClient;
        }
    }
} 