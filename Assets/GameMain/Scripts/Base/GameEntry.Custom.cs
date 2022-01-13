using UnityEngine;

namespace Game
{
    public partial class GameEntry : MonoBehaviour
    {
        public static TcpNetworkComponent TcpNetwork
        {
            get;
            private set;
        }
        
        /// <summary>
        /// TODO 测试服务器 现在是在Unity里面 比较方便 后期移植
        /// </summary>
        public static ServerComponent Server
        {
            get;
            private set;
        }

        /// <summary>
        /// 自定义 xLau模块
        /// </summary>
        public static LuaComponent Lua;

        /// <summary>
        /// 初始化自定义组件
        /// </summary>
        private static void InitCustomComponents()
        {
            TcpNetwork = UnityGameFramework.Runtime.GameEntry.GetComponent<TcpNetworkComponent>();
            Server = UnityGameFramework.Runtime.GameEntry.GetComponent<ServerComponent>();
            Lua = UnityGameFramework.Runtime.GameEntry.GetComponent<LuaComponent>();
            //TcpNetwork.StartConnect();
            
            Server.Init("127.0.0.1",18889);
        }
    }
}