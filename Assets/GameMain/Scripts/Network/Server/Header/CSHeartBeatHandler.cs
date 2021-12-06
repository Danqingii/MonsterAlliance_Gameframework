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
                Log.Error("服务器: CSHeartBeat 转换失败.");
            }
            else
            {
                Log.Info("服务器: 接收客户端心跳包 返回一个服务器心跳包");

                //给发送一个服务器心跳包
                GameEntry.Server.Send(ReferencePool.Acquire<SCHeartBeat>());
            }
        }
    }
}