using UnityEngine;

namespace Game
{
    public partial class GameEntry : MonoBehaviour
    {
        public static MongoDBComponent MongoDB
        {
            get;
            private set;
        }

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

        public static LuaComponent Xlua;

        /// <summary>
        /// 初始化自定义组件
        /// </summary>
        private static void InitCustomComponents()
        {
            MongoDB = UnityGameFramework.Runtime.GameEntry.GetComponent<MongoDBComponent>();
            TcpNetwork = UnityGameFramework.Runtime.GameEntry.GetComponent<TcpNetworkComponent>();
            Server = UnityGameFramework.Runtime.GameEntry.GetComponent<ServerComponent>();
            //Xlua = UnityGameFramework.Runtime.GameEntry.GetComponent<LuaComponent>();
            //TcpNetwork.StartConnect();
        }
    }
}