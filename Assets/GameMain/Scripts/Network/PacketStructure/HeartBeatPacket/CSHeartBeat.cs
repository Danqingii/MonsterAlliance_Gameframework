using ProtoBuf;
using System;

namespace Game
{
    /// <summary>
    /// 客户端-服务器 心跳包
    /// </summary>
    [Serializable, ProtoContract(Name = @"CSHeartBeat")]
    public class CSHeartBeat : CSPacketBase
    {
        public CSHeartBeat()
        {
        }

        [ProtoMember(1)]
        public override int Id
        {
            get
            {
                return 10000;
            }
        }

        public override void Clear()
        {
        }
    }
}