//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;

namespace GameFramework.Network
{
    internal sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        /// <summary>
        /// TCP 网络频道。
        /// </summary>
        private sealed class TcpNetworkChannel : NetworkChannelBase
        {
            //异步回调
            private readonly AsyncCallback m_ConnectCallback; //连接回调
            private readonly AsyncCallback m_SendCallback;    //发送回调
            private readonly AsyncCallback m_ReceiveCallback; //接收回调

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public TcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                m_ConnectCallback = ConnectCallback;
                m_SendCallback = SendCallback;
                m_ReceiveCallback = ReceiveCallback;
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public override ServiceType ServiceType
            {
                get
                {
                    return ServiceType.Tcp;
                }
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(IPAddress ipAddress, int port, object userData)
            {
                base.Connect(ipAddress, port, userData);
                m_Socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (m_Socket == null)
                {
                    string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                //开始连接回调 在连接前调用一次
                m_NetworkChannelHelper.PrepareForConnecting();
                
                //异步连接
                ConnectAsync(ipAddress, port, userData);
            }

            protected override bool ProcessSend()
            {
                //真正的发送消息  经过了base.ProcessSend() 处理 已经得到了序列化的流 也保证了流的信息
                if (base.ProcessSend())
                {
                    SendAsync();
                    return true;
                }

                return false;
            }

            private void ConnectAsync(IPAddress ipAddress, int port, object userData)
            {
                try
                {
                    //m_ConnectCallback = ConnectCallback
                    m_Socket.BeginConnect(ipAddress, port, m_ConnectCallback, new ConnectState(m_Socket, userData));
                }
                catch (Exception exception)
                {
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            //具体的连接回调
            private void ConnectCallback(IAsyncResult ar)
            {
                //得到一个连接状态
                ConnectState socketUserData = (ConnectState)ar.AsyncState;
                try
                {
                    //结束连接
                    socketUserData.Socket.EndConnect(ar);
                }
                
                //对已释放的对象执行操作时所引发的异常。
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                //重置 一些 可能留存的数据
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

                if (NetworkChannelConnected != null)
                {
                    //执行连接成功回调
                    NetworkChannelConnected(this, socketUserData.UserData);
                }

                //开始接受信息
                m_Active = true;
                ReceiveAsync();
            }

            private void SendAsync()
            {
                try
                {
                    //开始发送流信息
                    //具体发送的信息流
                    //从当前位置发送
                    //要发送的字节
                    //不对此调用使用任何标志
                    //发送回调
                    //自定义消息
                    m_Socket.BeginSend(m_SendState.Stream.GetBuffer(),
                        (int)m_SendState.Stream.Position, 
                        (int)(m_SendState.Stream.Length - m_SendState.Stream.Position), 
                        SocketFlags.None, 
                        m_SendCallback,
                        m_Socket);
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void SendCallback(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                if (!socket.Connected)
                {
                    return;
                }

                //发送成功会返回 发送的字节数
                int bytesSent = 0;
                try
                {
                    bytesSent = socket.EndSend(ar);
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                //这里是处理 如果这一条消息太长了  就继续发送
                m_SendState.Stream.Position += bytesSent;
                if (m_SendState.Stream.Position < m_SendState.Stream.Length)
                {
                    SendAsync();
                    return;
                }

                //发送完毕了 重置发送流
                m_SentPacketCount++;
                m_SendState.Reset();
                
                //到这里就是一整个 Tcp发送流程了 基本是非常的完整了
            }

            //接受消息回调
            private void ReceiveAsync()
            {
                try
                {
                    //接收流 也就是接受消息缓存区
                    //buffer数组中也就是消息缓存区接收数据的位置
                    //具体要接收的字节数
                    //不对此调用使用任何标志
                    //接受消息的回调
                    //用户定义的对象
                    m_Socket.BeginReceive(
                        m_ReceiveState.Stream.GetBuffer(),
                        (int)m_ReceiveState.Stream.Position, 
                        (int)(m_ReceiveState.Stream.Length - m_ReceiveState.Stream.Position),
                        SocketFlags.None,  
                        m_ReceiveCallback,
                        m_Socket);
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            //具体接受消息的回调
            private void ReceiveCallback(IAsyncResult ar)
            {
                Socket socket = (Socket)ar.AsyncState;
                if (!socket.Connected)
                {
                    return;
                }

                int bytesReceived = 0;
                try
                {
                    //得到接收的字节数
                    bytesReceived = socket.EndReceive(ar);
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                //因为 socket.EndReceive 会返回一个接收成功的数量 接收失败了就可以直接关闭连接了
                if (bytesReceived <= 0)
                {
                    Close();
                    return;
                }

                //这里也就是粘包黏包处理  把位置 + 长度
                m_ReceiveState.Stream.Position += bytesReceived;
                
                //如果流的位置小于了 流的长度  也就是说 包没有接收完全 继续接收
                if (m_ReceiveState.Stream.Position < m_ReceiveState.Stream.Length)
                {
                    ReceiveAsync();
                    return;
                }

                //------- 执行到了这里  一个完整的包接收完了
                //重置 接受流的位置

                m_ReceiveState.Stream.Position = 0L;

                //过程是否成功
                bool processSuccess = false;
                
                //包头并不是空的
                if (m_ReceiveState.PacketHeader != null) 
                {
                    //执行解包过程 也就是解包
                    processSuccess = ProcessPacket();
                    m_ReceivedPacketCount++;
                }
                else
                {
                    //执行步骤包头 也就是解包头
                    processSuccess = ProcessPacketHeader();
                }

                //成功的接收了 就继续接收
                if (processSuccess)
                {
                    ReceiveAsync();
                    return;
                }
            }
        }
    }
}
