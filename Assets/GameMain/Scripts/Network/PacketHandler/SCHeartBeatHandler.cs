using GameFramework.Network;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    /// 服务器-客户端 心跳包 处理者
    /// </summary>
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
            Log.Info("Receive packet '{0}'.", packetImpl.Id.ToString());
        }
    }
}