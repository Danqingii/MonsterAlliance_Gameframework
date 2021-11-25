namespace Game
{
    /// <summary>
    /// 数据库连接模式
    /// </summary>
    public enum ConnectionType : byte
    {
        /// <summary>
        /// 不会连接
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 本地连接
        /// </summary>
        LocalHost,
        
        /// <summary>
        /// 远程连接
        /// </summary>
        LongDistance
    }
}