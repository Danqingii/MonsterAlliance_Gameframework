using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//sc 处理结束之后 开始继续 操作
public class SCHeartBeatHandler : PacketHandlerBase
{
    public override int Id
    {
        get
        {
            return 2;
        }
    }

    public override void Handle(object sender, Packet packet)
    {
        SCHeartBeat packetImpl = (SCHeartBeat)packet;
        Log.Debug($"Receive packet '{packetImpl.Id.ToString()}'.");
    }
}
