

/// <summary>
/// 包头基类
/// </summary>
public abstract class PacketHeaderBase
{
    public abstract PacketType PacketType
    {
        get;
    }

    public abstract int Id
    {
        get;
        set;
    }

    public abstract int PacketLength
    {
        get;
        set;
    }

    public bool IsCompress
    {
        get
        {
            return PacketLength > 1024;
        }
    }

    public abstract uint Crc32
    {
        get;
        set;
    }

    public bool IsValid
    {
        get
        {
            return PacketType != PacketType.Undefined && Id > 0 && PacketLength >= 0;
        }
    }

    public void Clear()
    {
        Id = 0;
        PacketLength = 0;
    }
}