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
        /// 使用同步接收的 TCP 网络频道。
        /// </summary>
        private sealed class TcpWithSyncReceiveNetworkChannel : NetworkChannelBase
        {
            private readonly AsyncCallback m_ConnectCallback;  //连接回调
            private readonly AsyncCallback m_SendCallback;     //发送回调

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public TcpWithSyncReceiveNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                m_ConnectCallback = ConnectCallback;
                m_SendCallback = SendCallback;
            }

            /// <summary>
            /// 获取网络服务类型。
            /// </summary>
            public override ServiceType ServiceType
            {
                get
                {
                    return ServiceType.TcpWithSyncReceive;
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

                //执行开始连接回调
                m_NetworkChannelHelper.PrepareForConnecting();
                ConnectAsync(ipAddress, port, userData);
            }

            //程序发送
            protected override bool ProcessSend()
            {
                if (base.ProcessSend())
                {
                    SendAsync();
                    return true;
                }

                return false;
            }

            //程序接收
            protected override void ProcessReceive()
            {
                base.ProcessReceive();
                while (m_Socket.Available > 0)
                {
                    if (!ReceiveSync())
                    {
                        break;
                    }
                }
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

            //连接回调
            private void ConnectCallback(IAsyncResult ar)
            {
                //连接状态
                ConnectState socketUserData = (ConnectState)ar.AsyncState;
                try
                {
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

                //重置一些 可能存在的数据
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

                m_Active = true;
            }

            private void SendAsync()
            {
                try
                {
                    m_Socket.BeginSend(m_SendState.Stream.GetBuffer(), (int)m_SendState.Stream.Position, (int)(m_SendState.Stream.Length - m_SendState.Stream.Position), SocketFlags.None, m_SendCallback, m_Socket);
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

                m_SendState.Stream.Position += bytesSent;
                if (m_SendState.Stream.Position < m_SendState.Stream.Length)
                {
                    SendAsync();
                    return;
                }

                m_SentPacketCount++;
                m_SendState.Reset();
            }

            private bool ReceiveSync()
            {
                try
                {
                    int bytesReceived = m_Socket.Receive(m_ReceiveState.Stream.GetBuffer(), (int)m_ReceiveState.Stream.Position, (int)(m_ReceiveState.Stream.Length - m_ReceiveState.Stream.Position), SocketFlags.None);
                    if (bytesReceived <= 0)
                    {
                        Close();
                        return false;
                    }

                    m_ReceiveState.Stream.Position += bytesReceived;
                    if (m_ReceiveState.Stream.Position < m_ReceiveState.Stream.Length)
                    {
                        return false;
                    }

                    m_ReceiveState.Stream.Position = 0L;

                    bool processSuccess = false;
                    if (m_ReceiveState.PacketHeader != null)
                    {
                        processSuccess = ProcessPacket();
                        m_ReceivedPacketCount++;
                    }
                    else
                    {
                        processSuccess = ProcessPacketHeader();
                    }

                    return processSuccess;
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return false;
                    }

                    throw;
                }
            }
        }
    }
}
