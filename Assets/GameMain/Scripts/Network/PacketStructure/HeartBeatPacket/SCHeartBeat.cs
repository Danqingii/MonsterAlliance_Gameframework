using System;
using ProtoBuf;

namespace Game
{
    /// <summary>
    /// 服务器-客户端 心跳包
    /// </summary>
    public class SCHeartBeat : SCPacketBase
    {
        public SCHeartBeat()
        {
        }

        public override int Id
        {
            get
            {
                return 20000;
            }
        }

        public override void Clear()
        {
        }
    }
}