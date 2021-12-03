using GameFramework;
using GameFramework.Network;
using UnityGameFramework.Runtime;

namespace Game
{
    /// <summary>
    /// 客户端-服务器 心脏包处理者
    /// </summary>
    public class CSHeartBeatHandler : PacketHandlerBase
    {
        public override int Id
        {
            get
            {
                return PacketCoding.CSHeartBeat;
            }
        }

        public override void Handle(object sender, Packet packet)
        {
            CSHeartBeat packetImpl = (CSHeartBeat)packet;
            if (packetImpl == null)
            {
                Log.Error("CSHeartBeat 转换失败.");
            }
            else
            {
                Log.Info("服务器: Receive packet '{0}'.", packetImpl.Id.ToString());

                //发送一个心跳包
                //GameEntry.Server.Send(ReferencePool.Acquire<SCHeartBeat>());
            }
        }
    }
}