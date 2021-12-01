using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 包处理者 基类
/// </summary>
public abstract class PacketHandlerBase
{
    public abstract int Id
    {
        get;
    }

    public abstract void Handle(object sender, Packet packet);
}
