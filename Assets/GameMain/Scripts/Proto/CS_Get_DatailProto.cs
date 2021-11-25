//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2021-11-24 15:31:27
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 获取邮件详情
/// </summary>
public struct CS_Get_DatailProto : IProto
{
    public ushort ProtoCode { get { return 14004; } }

    public bool IsSuccess; //是否成功
    public string Name ; //邮件名字
    public ushort ErrorCode; //错误编码

    public byte[] ToArray()
    {
        using (CustomMemoryStream ms = new CustomMemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            ms.WriteBool(IsSuccess);
            if(IsSuccess)
            {
                ms.WriteUTF8String(Name );
            }
            else
            {
                ms.WriteUShort(ErrorCode);
            }
            return ms.ToArray();
        }
    }

    public static CS_Get_DatailProto GetProto(byte[] buffer)
    {
        CS_Get_DatailProto proto = new CS_Get_DatailProto();
        using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
        {
            proto.IsSuccess = ms.ReadBool();
            if(proto.IsSuccess)
            {
                proto.Name  = ms.ReadUTF8String();
            }
            else
            {
                proto.ErrorCode = ms.ReadUShort();
            }
        }
        return proto;
    }
}