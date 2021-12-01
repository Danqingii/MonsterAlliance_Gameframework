//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using GameFramework.Network;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game
{
    public class NetworkChannelHelper : INetworkChannelHelper
    {
        //服务器To客户端的 包类型
        private readonly Dictionary<int, Type> m_ServerToClientPacketTypes = new Dictionary<int, Type>();
        private readonly MemoryStream m_CachedStream = new MemoryStream(1024 * 8);   //缓存流 主要是为了缓存信息
        private INetworkChannel m_NetworkChannel = null;                             //具体的频道

        /// <summary>
        /// 获取消息包头长度 包头是固定的 从包头得到包体的解析 这里是4个字节。
        /// </summary>
        public int PacketHeaderLength
        {
            get
            {
                return sizeof(int);
            }
        }

        /// <summary>
        /// 初始化网络频道辅助器 也就是注册服务器包处理者 等待消息回调。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        public void Initialize(INetworkChannel networkChannel)
        {
            Log.Debug("TODO 初始化网络频道");
            
            m_NetworkChannel = networkChannel;

            //反射注册包和包处理函数。 也就是把服务器发给客户端的包全部都注册一下
            Type packetBaseType = typeof(SCPacketBase);               //服务器包-客户端包
            Type packetHandlerBaseType = typeof(PacketHandlerBase);   //包处理者
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (!types[i].IsClass || types[i].IsAbstract)
                {
                    continue;
                }
                
                if (types[i].BaseType == packetBaseType)
                {
                    PacketBase packetBase = (PacketBase)Activator.CreateInstance(types[i]);
                    Type packetType = GetServerToClientPacketType(packetBase.Id);
                    if (packetType != null)
                    {
                        Log.Warning("Already exist packet type '{0}', check '{1}' or '{2}'?.", packetBase.Id.ToString(), packetType.Name, packetBase.GetType().Name);
                        continue;
                    }

                    //包服务器-客户端的消息包注册进去
                    m_ServerToClientPacketTypes.Add(packetBase.Id, types[i]);
                }
                else if (types[i].BaseType == packetHandlerBaseType)
                {
                    //注册 包处理者
                    IPacketHandler packetHandler = (IPacketHandler)Activator.CreateInstance(types[i]);
                    m_NetworkChannel.RegisterHandler(packetHandler);
                }
            }

            GameEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkConnectedEventArgs.EventId, OnNetworkConnected);
            GameEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkClosedEventArgs.EventId, OnNetworkClosed);
            GameEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeat);
            GameEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkErrorEventArgs.EventId, OnNetworkError);
            GameEntry.Event.Subscribe(UnityGameFramework.Runtime.NetworkCustomErrorEventArgs.EventId, OnNetworkCustomError);
        }

        /// <summary>
        /// 关闭并清理网络频道辅助器。
        /// </summary>
        public void Shutdown()
        {
            GameEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkConnectedEventArgs.EventId, OnNetworkConnected);
            GameEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkClosedEventArgs.EventId, OnNetworkClosed);
            GameEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeat);
            GameEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkErrorEventArgs.EventId, OnNetworkError);
            GameEntry.Event.Unsubscribe(UnityGameFramework.Runtime.NetworkCustomErrorEventArgs.EventId, OnNetworkCustomError);

            m_NetworkChannel = null;
        }

        /// <summary>
        /// 准备进行连接。
        /// </summary>
        public void PrepareForConnecting()
        {
            m_NetworkChannel.Socket.ReceiveBufferSize = 1024 * 64;
            m_NetworkChannel.Socket.SendBufferSize = 1024 * 64;
        }

        /// <summary>
        /// 发送心跳消息包。
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        public bool SendHeartBeat()
        {
            m_NetworkChannel.Send(ReferencePool.Acquire<CSHeartBeat>());
            return true;
        }

        /// <summary>
        /// 序列化消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="packet">要序列化的消息包。</param>
        /// <param name="destination">要序列化的目标流。</param>
        /// <returns>是否序列化成功。</returns>
        public bool Serialize<T>(T packet, Stream destination) where T : Packet
        {
            PacketBase packetImpl = packet as PacketBase;
            if (packetImpl == null)
            {
                Log.Warning("Packet is invalid.");
                return false;
            }

            if (packetImpl.PacketType != PacketType.ClientToServer)
            {
                Log.Warning("Send packet invalid.");
                return false;
            }

            // 因为头部消息有8字节长度，所以先跳过8字节
            m_CachedStream.SetLength(m_CachedStream.Capacity);
            m_CachedStream.Position = 8;
            Serializer.SerializeWithLengthPrefix(m_CachedStream, packet, PrefixStyle.Fixed32);
            //序列化包 因为

            // 头部消息
            CSPacketHeader packetHeader = ReferencePool.Acquire<CSPacketHeader>();
            packetHeader.Id = packet.Id;
            packetHeader.PacketLength = (int)destination.Length - 8; // 消息内容长度需要减去头部消息长度

            destination.Position = 0;
            Serializer.SerializeWithLengthPrefix(m_CachedStream, packetHeader, PrefixStyle.Fixed32);
            
            UnityEngine.Debug.Log($"{packetHeader.Id}");
            UnityEngine.Debug.Log($"{packetHeader.PacketLength}");
            UnityEngine.Debug.Log($"{(packet as CSLogin).Account}");
            UnityEngine.Debug.Log($"{(packet as CSLogin).Password}");
            
            /*
            m_CachedStream.SetLength(m_CachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            m_CachedStream.Position = 0L;
            
            //序列化包体
            Serializer.SerializeWithLengthPrefix(m_CachedStream, packet, PrefixStyle.Fixed32);

            CSPacketHeader packetHeader = ReferencePool.Acquire<CSPacketHeader>();
            packetHeader.Id = packet.Id;
            packetHeader.PacketLength = m_CachedStream.GetBuffer().Length;
            
            UnityEngine.Debug.Log($"{packetHeader.Id}");
            UnityEngine.Debug.Log($"{packetHeader.PacketLength}");
            UnityEngine.Debug.Log($"{(packet as CSLogin).Account}");
            UnityEngine.Debug.Log($"{(packet as CSLogin).Password}");
            
            Serializer.Serialize(m_CachedStream, packetHeader);
            
            
            ReferencePool.Release(packetHeader);
            ReferencePool.Release((IReference)packet);
            */
            
            UnityEngine.Debug.Log($"发送包:{packet.GetType().FullName}");

            //重中之中 把序列化的结果 又写入到了 目标流:destination
            m_CachedStream.WriteTo(destination);
            return true;
        }

        /// <summary>
        /// 反序列化消息包头。
        /// </summary>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包头。</returns>
        public IPacketHeader DeserializePacketHeader(Stream source, out object customErrorData)
        {
            // 反序列化包头是在解包的异步调用的！
            // 注意：此函数并不在主线程调用！
            customErrorData = null;
            
            //protobuf 的反序列化包头
            return (IPacketHeader)RuntimeTypeModel.Default.Deserialize(source, ReferencePool.Acquire<SCPacketHeader>(), typeof(SCPacketHeader));
        }

        /// <summary>
        /// 反序列化消息包。
        /// </summary>
        /// <param name="packetHeader">消息包头。</param>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包。</returns>
        public Packet DeserializePacket(IPacketHeader packetHeader, Stream source, out object customErrorData)
        {
            // 注意：此函数并不在主线程调用！
            customErrorData = null;

            //把具体的包头转换一下
            SCPacketHeader scPacketHeader = packetHeader as SCPacketHeader;
            if (scPacketHeader == null)
            {
                Log.Warning("Packet header is invalid.");
                return null;
            }

            //从包头得到具体有用的信息
            Packet packet = null;
            if (scPacketHeader.IsValid)
            {
                //这里可以做一系列的处理
                
                //数据包是否压缩
                
                //数据包是否丢失  crc32验证
                
                //数据包是否加密  各种验证 
                
                //TODO 经过了上面所有的操作 得到真正的数据包 然后解析
                
                //如果不存在 注册过的 服务器-客户端包  爆警告
                Type packetType = GetServerToClientPacketType(scPacketHeader.Id);
                if (packetType != null)
                {
                    packet = (Packet)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(source, ReferencePool.Acquire(packetType), packetType, PrefixStyle.Fixed32, 0);
                }
                else
                {
                    Log.Warning("Can not deserialize packet for packet id '{0}'.", scPacketHeader.Id.ToString());
                }
            }
            else
            {
                Log.Warning("Packet header is invalid.");
            }

            ReferencePool.Release(scPacketHeader);
            return packet;
        }

        private Type GetServerToClientPacketType(int id)
        {
            Type type = null;
            if (m_ServerToClientPacketTypes.TryGetValue(id, out type))
            {
                return type;
            }

            return null;
        }

        #region 事件回调

        private void OnNetworkConnected(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkConnectedEventArgs ne = (UnityGameFramework.Runtime.NetworkConnectedEventArgs)e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' connected, local address '{1}', remote address '{2}'.", ne.NetworkChannel.Name, ne.NetworkChannel.Socket.LocalEndPoint.ToString(), ne.NetworkChannel.Socket.RemoteEndPoint.ToString());
        }

        private void OnNetworkClosed(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkClosedEventArgs ne = (UnityGameFramework.Runtime.NetworkClosedEventArgs)e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' closed.", ne.NetworkChannel.Name);
        }

        private void OnNetworkMissHeartBeat(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs ne = (UnityGameFramework.Runtime.NetworkMissHeartBeatEventArgs)e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' miss heart beat '{1}' times.", ne.NetworkChannel.Name, ne.MissCount.ToString());

            //如果心跳包mis
            if (ne.MissCount < 2)
            {
                return;
            }

            ne.NetworkChannel.Close();
        }

        private void OnNetworkError(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkErrorEventArgs ne = (UnityGameFramework.Runtime.NetworkErrorEventArgs)e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", ne.NetworkChannel.Name, ne.ErrorCode.ToString(), ne.ErrorMessage);
            
            ne.NetworkChannel.Close();
        }

        private void OnNetworkCustomError(object sender, GameEventArgs e)
        {
            UnityGameFramework.Runtime.NetworkCustomErrorEventArgs ne = (UnityGameFramework.Runtime.NetworkCustomErrorEventArgs)e;
            if (ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }
        }

        #endregion
    }
}
