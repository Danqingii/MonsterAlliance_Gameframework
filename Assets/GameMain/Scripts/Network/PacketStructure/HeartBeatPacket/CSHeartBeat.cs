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
        public override int Id
        {
            get
            {
                return PacketCoding.CSHeartBeat;
            }
        }
        
        /*private bool m_HeartBeat = true;

        [ProtoMember(1)]
        public bool HeartBeat
        {
            get
            {
                return m_HeartBeat;
            }
            set
            {
                m_HeartBeat = value;
            }
        }*/

        public override void Clear()
        {
        }
    }
}