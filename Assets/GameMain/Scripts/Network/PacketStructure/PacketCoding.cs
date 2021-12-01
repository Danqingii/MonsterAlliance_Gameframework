namespace Game
{
    /// <summary>
    /// 包编码  我们定一个小协议
    /// 客户端-服务器 都是10000
    /// 服务器-客户端 都是20000
    /// </summary>
    public static class PacketCoding
    {
        public const int CSHeartBeat = 10000;
        public const int SCHeartBeat = 20000;
        
        public const int CSLogin = 10001;
        public const int SCLogin = 20001;
    }
}