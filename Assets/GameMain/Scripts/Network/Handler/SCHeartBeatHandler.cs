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
                return PacketCoding.SCHeartBeat;
            }
        }

        public override void Handle(object sender, Packet packet)
        {
            SCHeartBeat packetImpl = (SCHeartBeat)packet;
            if (packetImpl == null)
            {
                Log.Error("客户端: SCHeartBeat 转换失败.");
            }
            else
            {
                Log.Info("客户端: 接收服务器心跳包");
            }
        }
    }
}