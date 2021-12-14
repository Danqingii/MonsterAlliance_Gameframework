using GameFramework.Event;

namespace Game
{
    /// <summary>
    /// 加载 Lua 脚本失败事件。
    /// </summary>
    public sealed class LoadLuaScriptFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 加载 Lua 脚本失败事件编号。
        /// </summary>
        public static readonly int EventId = typeof(LoadLuaScriptFailureEventArgs).GetHashCode();

        /// <summary>
        /// 获取加载 Lua 脚本失败事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取 Lua 脚本名称。
        /// </summary>
        public string LuaScriptName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取 Lua 脚本资源名称。
        /// </summary>
        public string LuaScriptAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 清理加载 Lua 脚本失败事件。
        /// </summary>
        public override void Clear()
        {
            LuaScriptName = default(string);
            LuaScriptAssetName = default(string);
            ErrorMessage = default(string);
            UserData = default(object);
        }

        /// <summary>
        /// 填充加载 Lua 脚本失败事件。
        /// </summary>
        /// <param name="luaScriptName">Lua 脚本名称。</param>
        /// <param name="luaScriptAssetName">Lua 脚本资源名称。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>加载 Lua 脚本失败事件。</returns>
        public LoadLuaScriptFailureEventArgs Fill(string luaScriptName, string luaScriptAssetName, string errorMessage, object userData)
        {
            LuaScriptName = luaScriptName;
            LuaScriptAssetName = luaScriptAssetName;
            ErrorMessage = errorMessage;
            UserData = userData;

            return this;
        }
    }
}
