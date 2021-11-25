/// <summary>
/// 测试协议
/// </summary>
public struct TestProto : IProto
{
    public ushort ProtoCode
    {
        get
        {
            return 1001;
        }
    }
    
    public int Id;
    public string Name;
    public float Price;
    public int Numr;

    public byte[] ToArray()
    {
        using (CustomMemoryStream ms = new CustomMemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteInt(Id);
            ms.WriteUTF8String(Name);
            ms.WriteFloat(Price);
            ms.WriteInt(Numr);
            return ms.ToArray();
        }
    }

    public static TestProto GetProto(byte[] buffer)
    {
        TestProto proto = new TestProto();
        
        using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
        {
            proto.Id =  ms.ReadInt();
            proto.Name =  ms.ReadUTF8String();
            proto.Price = ms.ReadFloat();
            proto.Numr = ms.ReadInt();
        }
        return proto;
    }
}