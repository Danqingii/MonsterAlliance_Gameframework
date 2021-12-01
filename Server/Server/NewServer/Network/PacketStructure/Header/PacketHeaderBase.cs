

/// <summary>
/// 包头基类
/// </summary>
public abstract class PacketHeaderBase
{
    public abstract PacketType PacketType
    {
        get;
    }

    public virtual int Id
    {
        get;
        set;
    }

    public virtual int PacketLength
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