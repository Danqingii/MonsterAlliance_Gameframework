using GameFramework.Event;

namespace Game
{
    /// <summary>
    /// 加载 Lua 脚本成功事件。
    /// </summary>
    public sealed class LoadLuaScriptSuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 加载 Lua 脚本成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(LoadLuaScriptSuccessEventArgs).GetHashCode();

        /// <summary>
        /// 获取加载 Lua 脚本成功事件编号。
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
        /// 获取加载持续时间。
        /// </summary>
        public float Duration
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
        /// 清理加载 Lua 脚本成功事件。
        /// </summary>
        public override void Clear()
        {
            LuaScriptName = default(string);
            LuaScriptAssetName = default(string);
            Duration = default(float);
            UserData = default(object);
        }

        /// <summary>
        /// 填充加载 Lua 脚本成功事件。
        /// </summary>
        /// <param name="luaScriptName">Lua 脚本名称。</param>
        /// <param name="luaScriptAssetName">Lua 脚本资源名称。</param>
        /// <param name="duration">加载持续时间。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>加载 Lua 脚本成功事件。</returns>
        public LoadLuaScriptSuccessEventArgs Fill(string luaScriptName, string luaScriptAssetName, float duration, object userData)
        {
            LuaScriptName = luaScriptName;
            LuaScriptAssetName = luaScriptAssetName;
            Duration = duration;
            UserData = userData;

            return this;
        }
    }
}
