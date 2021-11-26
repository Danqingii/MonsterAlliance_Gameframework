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

        public static LuaComponent Xlua;

        /// <summary>
        /// 初始化自定义组件
        /// </summary>
        private static void InitCustomComponents()
        {
            MongoDB = UnityGameFramework.Runtime.GameEntry.GetComponent<MongoDBComponent>();
            TcpNetwork = UnityGameFramework.Runtime.GameEntry.GetComponent<TcpNetworkComponent>();
            //Xlua = UnityGameFramework.Runtime.GameEntry.GetComponent<LuaComponent>();

            //TcpNetwork.StartConnect();
        }
    }
}