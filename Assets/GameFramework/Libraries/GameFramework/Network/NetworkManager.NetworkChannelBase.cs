//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        /// <summary>
        /// 网络频道基类。
        /// </summary>
        private abstract class NetworkChannelBase : INetworkChannel, IDisposable
        {
            //默认心跳包间隔
            private const float DefaultHeartBeatInterval = 30f;

            private readonly string m_Name;
            protected readonly Queue<Packet> m_SendPacketPool;               //发送包的池子
            protected readonly EventPool<Packet> m_ReceivePacketPool;        //接收包池子
            protected readonly INetworkChannelHelper m_NetworkChannelHelper; //频道助手
            protected AddressFamily m_AddressFamily;                         //频道的网络类型
            protected bool m_ResetHeartBeatElapseSecondsWhenReceivePacket;   //当收到消息包时是否重置心跳流逝时间。
            protected float m_HeartBeatInterval;                             //心跳间隔时长
            protected Socket m_Socket;                                       //具体的Socket
            protected readonly SendState m_SendState;                        //发送状态  包的处理 其实也就是缓存器
            protected readonly ReceiveState m_ReceiveState;                  //接收状态  存有接收包  接收包头
            protected readonly HeartBeatState m_HeartBeatState;              //心跳状态  心跳包状态
            protected int m_SentPacketCount;                                 //发送的包数量
            protected int m_ReceivedPacketCount;                             //接收的包数量
            protected bool m_Active;                                         //是否行动的
            private bool m_Disposed;                                         //是否销毁的

            
            //这5个回调都是为了 外部执行一些信息
            /// <summary>网络频道连接</summary>
            public GameFrameworkAction<NetworkChannelBase, object> NetworkChannelConnected;
            
            /// <summary>网络连接关闭</summary>
            public GameFrameworkAction<NetworkChannelBase> NetworkChannelClosed;
            
            /// <summary>网络频道心跳包未发送</summary>
            public GameFrameworkAction<NetworkChannelBase, int> NetworkChannelMissHeartBeat;
            
            /// <summary>网络频道错误</summary>
            public GameFrameworkAction<NetworkChannelBase, NetworkErrorCode, SocketError, string> NetworkChannelError;
            
            /// <summary>自定义网络频道错误</summary>
            public GameFrameworkAction<NetworkChannelBase, object> NetworkChannelCustomError;

            /// <summary>
            /// 初始化网络频道基类的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public NetworkChannelBase(string name, INetworkChannelHelper networkChannelHelper)
            {
                m_Name = name ?? string.Empty;
                m_SendPacketPool = new Queue<Packet>();
                m_ReceivePacketPool = new EventPool<Packet>(EventPoolMode.Default);
                m_NetworkChannelHelper = networkChannelHelper;
                m_AddressFamily = AddressFamily.Unknown;
                m_ResetHeartBeatElapseSecondsWhenReceivePacket = false;
                m_HeartBeatInterval = DefaultHeartBeatInterval;
                m_Socket = null;
                m_SendState = new SendState();
                m_ReceiveState = new ReceiveState();
                m_HeartBeatState = new HeartBeatState();
                m_SentPacketCount = 0;
                m_ReceivedPacketCount = 0;
                m_Active = false;
                m_Disposed = false;

                NetworkChannelConnected = null;
                NetworkChannelClosed = null;
                NetworkChannelMissHeartBeat = null;
                NetworkChannelError = null;
                NetworkChannelCustomError = null;

                networkChannelHelper.Initialize(this);
            }

            /// <summary>
            /// 获取网络频道名称。
            /// </summary>
            public string Name
            {
                get
                {
                    return m_Name;
                }
            }

            /// <summary>
            /// 获取网络频道所使用的 Socket。
            /// </summary>
            public Socket Socket
            {
                get
                {
                    return m_Socket;
                }
            }

            /// <summary>
            /// 获取是否已连接。
            /// </summary>
            public bool Connected
            {
                get
                {
                    if (m_Socket != null)
                    {
                        return m_Socket.Connected;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public abstract ServiceType ServiceType
            {
                get;
            }

            /// <summary>
            /// 获取网络地址类型。
            /// </summary>
            public AddressFamily AddressFamily
            {
                get
                {
                    return m_AddressFamily;
                }
            }

            /// <summary>
            /// 获取要发送的消息包数量。
            /// </summary>
            public int SendPacketCount
            {
                get
                {
                    return m_SendPacketPool.Count;
                }
            }

            /// <summary>
            /// 获取累计发送的消息包数量。
            /// </summary>
            public int SentPacketCount
            {
                get
                {
                    return m_SentPacketCount;
                }
            }

            /// <summary>
            /// 获取已接收未处理的消息包数量。
            /// </summary>
            public int ReceivePacketCount
            {
                get
                {
                    return m_ReceivePacketPool.EventCount;
                }
            }

            /// <summary>
            /// 获取累计已接收的消息包数量。
            /// </summary>
            public int ReceivedPacketCount
            {
                get
                {
                    return m_ReceivedPacketCount;
                }
            }

            /// <summary>
            /// 获取或设置当收到消息包时是否重置心跳流逝时间。
            /// </summary>
            public bool ResetHeartBeatElapseSecondsWhenReceivePacket
            {
                get
                {
                    return m_ResetHeartBeatElapseSecondsWhenReceivePacket;
                }
                set
                {
                    m_ResetHeartBeatElapseSecondsWhenReceivePacket = value;
                }
            }

            /// <summary>
            /// 获取丢失心跳的次数。
            /// </summary>
            public int MissHeartBeatCount
            {
                get
                {
                    return m_HeartBeatState.MissHeartBeatCount;
                }
            }

            /// <summary>
            /// 获取或设置心跳间隔时长，以秒为单位。
            /// </summary>
            public float HeartBeatInterval
            {
                get
                {
                    return m_HeartBeatInterval;
                }
                set
                {
                    m_HeartBeatInterval = value;
                }
            }

            /// <summary>
            /// 获取心跳等待时长，以秒为单位。
            /// </summary>
            public float HeartBeatElapseSeconds
            {
                get
                {
                    return m_HeartBeatState.HeartBeatElapseSeconds;
                }
            }

            /// <summary>
            /// 网络频道轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public virtual void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (m_Socket == null || !m_Active)
                {
                    return;
                }

                ProcessSend();   //程序发送 也就是包发送
                ProcessReceive();//程序接收 包接收
                if (m_Socket == null || !m_Active)
                {
                    return;
                }

                //接收消息包池  为啥也要Update!!!  不是已经在EventManager里面轮询了吗
                m_ReceivePacketPool.Update(elapseSeconds, realElapseSeconds);

                if (m_HeartBeatInterval > 0f)
                {
                    bool sendHeartBeat = false;  //发送心跳包
                    int missHeartBeatCount = 0;  //未成功的心跳包数量
                    lock (m_HeartBeatState)
                    {
                        if (m_Socket == null || !m_Active)
                        {
                            return;
                        }

                        m_HeartBeatState.HeartBeatElapseSeconds += realElapseSeconds;
                        
                        //如果心跳包的时间 大于了心跳包间隔时间  代表客户端与服务器 未通信
                        if (m_HeartBeatState.HeartBeatElapseSeconds >= m_HeartBeatInterval)
                        {
                            sendHeartBeat = true;
                            missHeartBeatCount = m_HeartBeatState.MissHeartBeatCount;
                            m_HeartBeatState.HeartBeatElapseSeconds = 0f;
                            m_HeartBeatState.MissHeartBeatCount++;
                        }
                    }

                    //如果心跳包未发送 执行心跳包miss 回调
                    if (sendHeartBeat && m_NetworkChannelHelper.SendHeartBeat())
                    {
                        if (missHeartBeatCount > 0 && NetworkChannelMissHeartBeat != null)
                        {
                            NetworkChannelMissHeartBeat(this, missHeartBeatCount);
                        }
                    }
                }
            }

            /// <summary>
            /// 关闭网络频道。
            /// </summary>
            public virtual void Shutdown()
            {
                Close();
                m_ReceivePacketPool.Shutdown();
                m_NetworkChannelHelper.Shutdown();
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数。</param>
            public void RegisterHandler(IPacketHandler handler)
            {
                if (handler == null)
                {
                    throw new GameFrameworkException("Packet handler is invalid.");
                }

                m_ReceivePacketPool.Subscribe(handler.Id, handler.Handle);
            }

            /// <summary>
            /// 设置默认事件处理函数。
            /// </summary>
            /// <param name="handler">要设置的默认事件处理函数。</param>
            public void SetDefaultHandler(EventHandler<Packet> handler)
            {
                m_ReceivePacketPool.SetDefaultHandler(handler);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            public void Connect(IPAddress ipAddress, int port)
            {
                Connect(ipAddress, port, null);
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public virtual void Connect(IPAddress ipAddress, int port, object userData)
            {
                if (m_Socket != null)
                {
                    Close();
                    m_Socket = null;
                }

                switch (ipAddress.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        m_AddressFamily = AddressFamily.IPv4;
                        break;

                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        m_AddressFamily = AddressFamily.IPv6;
                        break;

                    default:
                        string errorMessage = Utility.Text.Format("Not supported address family '{0}'.", ipAddress.AddressFamily);
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.AddressFamilyError, SocketError.Success, errorMessage);
                            return;
                        }

                        throw new GameFrameworkException(errorMessage);
                }

                //重置一下发送 和接收  状态
                m_SendState.Reset();
                m_ReceiveState.PrepareForPacketHeader(m_NetworkChannelHelper.PacketHeaderLength);
            }

            /// <summary>
            /// 关闭连接并释放所有相关资源。
            /// </summary>
            public void Close()
            {
                lock (this)
                {
                    if (m_Socket == null)
                    {
                        return;
                    }

                    m_Active = false;

                    try
                    {
                        m_Socket.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        m_Socket.Close();
                        m_Socket = null;

                        if (NetworkChannelClosed != null)
                        {
                            NetworkChannelClosed(this);
                        }
                    }

                    m_SentPacketCount = 0;
                    m_ReceivedPacketCount = 0;

                    lock (m_SendPacketPool)
                    {
                        m_SendPacketPool.Clear();
                    }

                    m_ReceivePacketPool.Clear();

                    lock (m_HeartBeatState)
                    {
                        m_HeartBeatState.Reset(true);
                    }
                }
            }

            /// <summary>
            /// 向远程主机发送消息包。
            /// </summary>
            /// <typeparam name="T">消息包类型。</typeparam>
            /// <param name="packet">要发送的消息包。</param>
            public void Send<T>(T packet) where T : Packet
            {
                if (m_Socket == null)
                {
                    string errorMessage = "You must connect first.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!m_Active)
                {
                    string errorMessage = "Socket is not active.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (packet == null)
                {
                    string errorMessage = "Packet is invalid.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                lock (m_SendPacketPool)
                {
                    //把包添加进入 准备发送包池中
                    m_SendPacketPool.Enqueue(packet);
                }
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            /// <param name="disposing">释放资源标记。</param>
            private void Dispose(bool disposing)
            {
                if (m_Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    Close();
                    m_SendState.Dispose();
                    m_ReceiveState.Dispose();
                }

                m_Disposed = true;
            }

            //发送过程  一直在Update里面发送
            protected virtual bool ProcessSend()
            {
                //流的长度 >0 才代表不会空数据        发送包的池 小于0 也是false
                if (m_SendState.Stream.Length > 0 || m_SendPacketPool.Count <= 0)
                {
                    return false;
                }

                while (m_SendPacketPool.Count > 0)
                {
                    Packet packet = null;
                    lock (m_SendPacketPool)
                    {
                        packet = m_SendPacketPool.Dequeue();
                    }

                    //序列化结果
                    bool serializeResult = false;
                    try
                    {
                        //序列化的同时 估计已经发送了消息包了
                        serializeResult = m_NetworkChannelHelper.Serialize(packet, m_SendState.Stream);
                    }
                    catch (Exception exception)
                    {
                        m_Active = false;
                        if (NetworkChannelError != null)
                        {
                            SocketException socketException = exception as SocketException;
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                            return false;
                        }

                        throw;
                    }

                    if (!serializeResult)
                    {
                        string errorMessage = "Serialized packet failure.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.SerializeError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }
                }

                m_SendState.Stream.Position = 0L;
                return true;
            }

            //接收过程 一直在Update 里面接收
            protected virtual void ProcessReceive()
            {
            }

            //步骤消息包头
            protected virtual bool ProcessPacketHeader() 
            {
                try
                {
                    object customErrorData = null;
                    IPacketHeader packetHeader = m_NetworkChannelHelper.DeserializePacketHeader(m_ReceiveState.Stream, out customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    //如果包头为空
                    if (packetHeader == null)
                    {
                        string errorMessage = "Packet header is invalid.";
                        if (NetworkChannelError != null)
                        {
                            NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, errorMessage);
                            return false;
                        }

                        throw new GameFrameworkException(errorMessage);
                    }

                    //包头不为空 缓存一下包头
                    m_ReceiveState.PrepareForPacket(packetHeader);
                    
                    //如果包头长度为0 也就是可以直接发送包  不发送包头了
                    if (packetHeader.PacketLength <= 0)
                    {
                        bool processSuccess = ProcessPacket();
                        m_ReceivedPacketCount++;
                        return processSuccess;
                    }
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketHeaderError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }

            //步骤包 
            protected virtual bool ProcessPacket()
            {
                lock (m_HeartBeatState)
                {
                    //发包之前 重置一下 心跳包
                    m_HeartBeatState.Reset(m_ResetHeartBeatElapseSecondsWhenReceivePacket);
                }

                try
                {
                    //真正的包
                    object customErrorData = null;
                    Packet packet = m_NetworkChannelHelper.DeserializePacket(m_ReceiveState.PacketHeader, m_ReceiveState.Stream, out customErrorData);

                    if (customErrorData != null && NetworkChannelCustomError != null)
                    {
                        NetworkChannelCustomError(this, customErrorData);
                    }

                    //开始分发 具体的包
                    if (packet != null)
                    {
                        m_ReceivePacketPool.Fire(this, packet);
                    }

                    //分发了包之后重置一下 接收包的状态 把里面的数据给重置掉
                    m_ReceiveState.PrepareForPacketHeader(m_NetworkChannelHelper.PacketHeaderLength);
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        //反序列化包 出现错误
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }

                return true;
            }
        }
    }
}
