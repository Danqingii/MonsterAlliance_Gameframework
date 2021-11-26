using GameFramework.Network;

namespace Game
{
    /// <summary>
    /// 包处理者 基类
    /// </summary>
    public abstract class PacketHandlerBase : IPacketHandler
    {
        public abstract int Id
        {
            get;
        }

        public abstract void Handle(object sender, Packet packet);
    }
}