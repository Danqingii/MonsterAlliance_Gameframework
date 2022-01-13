using UnityGameFramework.Runtime;

namespace Game
{
    public sealed partial class LuaComponent : GameFrameworkComponent
    {
        /// <summary>
        /// lua脚本信息
        /// </summary>
        private sealed class LuaScriptInfo
        {
            private readonly string m_LuaScriptName;
            private readonly object m_UserData;

            public LuaScriptInfo(string luaScriptName, object userData)
            {
                m_LuaScriptName = luaScriptName;
                m_UserData = userData;
            }

            /// <summary>
            /// lua脚本名字
            /// </summary>
            public string LuaScriptName
            {
                get
                {
                    return m_LuaScriptName;
                }
            }

            /// <summary>
            /// 用户自定义信息
            /// </summary>
            public object UserData
            {
                get
                {
                    return m_UserData;
                }
            }
        }
    }
}