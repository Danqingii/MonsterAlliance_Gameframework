using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 登陆协议
/// </summary>
public struct CS_LoginProto : IProto
{
    public ushort ProtoCode { get { return 10001; } }

    public string Id; //账号
    public string Pw; //密码

    public byte[] ToArray()
    {
        using (CustomMemoryStream ms = new CustomMemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteUTF8String(Id);
            ms.WriteUTF8String(Pw);
            return ms.ToArray();
        }
    }

    public static CS_LoginProto GetProto(byte[] buffer)
    {
        CS_LoginProto proto = new CS_LoginProto();
        using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
        {
            proto.Id = ms.ReadUTF8String();
            proto.Pw = ms.ReadUTF8String();
        }
        return proto;
    }
}