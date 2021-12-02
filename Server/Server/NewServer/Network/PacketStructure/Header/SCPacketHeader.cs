using ProtoBuf;

/// <summary>
/// 服务器发送给客户端的 包头
/// </summary>
public sealed class SCPacketHeader : PacketHeaderBase
{
    public override PacketType PacketType
    {
        get
        {
            return PacketType.ServerToClient;
        }
    }

    [ProtoMember(1)]
    public override int Id
    {
        get;
        set;
    }

    [ProtoMember(2)]
    public override int PacketLength
    {
        get;
        set;
    }

    [ProtoMember(3)]
    public override uint Crc32
    {
        get;
        set;
    }
}