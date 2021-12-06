using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Game;
using GameFramework;
using GameFramework.Network;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;
using UnityGameFramework.Runtime;
using AddressFamily = System.Net.Sockets.AddressFamily;

/// <summary>
/// 服务器 --TODO unity里面 模拟服务器的 收发
/// </summary>
public partial class ServerComponent : GameFrameworkComponent
{
     private readonly Dictionary<int, Type> m_ClientToServerPacketTypes = new Dictionary<int, Type>();  //客户端To服务器的包
     private readonly PackDispatche m_PackDispatche = new PackDispatche();                              //事件派发

     private Socket m_ServerSocket;    //测试服务器
     private Socket m_ClientSocket;    //连接的客户端

     private Thread m_ReceiveTherad;   //接收客户端的线程
     private byte[] m_ReceiveBuffer;   //接收数据包的字节缓冲数据流
     
     private MemoryStream m_ReceiveState;                              //接收状态流
     private MemoryStream m_SendState;                                 //发送状态流
     
     private readonly Queue<Packet> m_SendQuene = new Queue<Packet>(); //发送消息队列
     private Action m_CheckSendQuene;                                  //核查队列的委托

     public MongoManager MongoManager;
     
     //包头的长度 现在是12  固定的包头长度 包头的长度是跟随 protobuf的 如果压缩不到位 很容易出问题
     private int PacketHeaderLength
     {
          get
          {
               return sizeof(int) * 3;
          }
     }
     
     protected override void Awake()
     {
          base.Awake();
          
          m_ReceiveBuffer = new byte[1024 * 10];
          m_ReceiveState = new MemoryStream();
          m_SendState = new MemoryStream(1024 * 10);
         
          m_CheckSendQuene = OnCheckSendQuene;
     }

     private void Start()
     {
          Type packetBaseType = typeof(CSPacketBase);               //客户端包-服务器包
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
                    Type packetType = GetClientToServerPacketType(packetBase.Id);
                    if (packetType != null)
                    {
                         Log.Warning("服务器:Already exist packet type '{0}', check '{1}' or '{2}'?.", packetBase.Id.ToString(), packetType.Name, packetBase.GetType().Name);
                         continue;
                    }

                    //客户端-服务器的消息包注册进去
                    m_ClientToServerPacketTypes.Add(packetBase.Id, types[i]);
               }
               else if (types[i].BaseType == packetHandlerBaseType) //处理者
               {
                    IPacketHandler packetHandler = (IPacketHandler)Activator.CreateInstance(types[i]);
                    
                    //关键一步 注册服务器处理消息分发 因为我们得不到GF 内置的EventPool 自己我们写一个模拟一下
                    m_PackDispatche.Subscribe(packetHandler.Id, packetHandler.Handle);
               }
          }
     }
     
     private void OnDestroy()
     {
          m_ClientSocket?.Close();
          m_ServerSocket?.Close();
          m_ReceiveTherad?.Abort();
     }

     public void Init(string ip,int port)
     {
          Log.Info("Open Local Server");

          m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          m_ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
          m_ServerSocket.Listen(1);
          
          m_ReceiveTherad = new Thread(ListenClientCallBack);
          m_ReceiveTherad.IsBackground = true;
          m_ReceiveTherad.Start();

          //初始化数据库
          MongoManager = new MongoManager();
          MongoManager.Init();
     }
     
     private void ListenClientCallBack()
     {
          while (true)
          {
               m_ClientSocket = m_ServerSocket.Accept();
               Log.Info($"服务器:客户端{m_ClientSocket.RemoteEndPoint}已经连接");

               Thread thread = new Thread(ReceiveMessage);
               thread.IsBackground = true;
               thread.Start();
          }
     }
     
     private void ReceiveMessage()
     {
          m_ClientSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, OnReceiveCallback, m_ClientSocket);
     }

     private void OnReceiveCallback(IAsyncResult ar)
     {
          Socket socket = (Socket)ar.AsyncState;
          try
          {
               int bytesReceived = socket.EndReceive(ar);

               if (bytesReceived > 0)
               {
                    //把接收到的数据 写入缓冲数据流的尾部
                    m_ReceiveState.Position = m_ReceiveState.Length;

                    m_ReceiveState.Write(m_ReceiveBuffer, 0, bytesReceived);

                    //如果缓存数据流的长度大于包头的长度  至少有一个不完整的包过来了
                    if (m_ReceiveState.Length > PacketHeaderLength)
                    {
                         while (true)
                         {
                              m_ReceiveState.Position = 0;
                              CSPacketHeader header = Serializer.DeserializeWithLengthPrefix<CSPacketHeader>(m_ReceiveState, PrefixStyle.Fixed32);
                              if (header == null)
                              {
                                   Log.Error("服务器:反序列化失败");
                              }
                              else
                              {
                                   int packetBodyLen = header.PacketLength; //包体长度
                                   int fullPacketLen = packetBodyLen + PacketHeaderLength;   //整包长度

                                   //有一个完整包
                                   if (m_ReceiveState.Length >= fullPacketLen)
                                   {
                                        //容纳包体的byte[] 缓存区
                                        byte[] packetBodyBuffer = new byte[packetBodyLen];

                                        //把包的指向移动至包体处
                                        m_ReceiveState.Position = PacketHeaderLength;
                                        
                                        //读取全部的包体数据至缓存区
                                        m_ReceiveState.Read(packetBodyBuffer, 0, packetBodyLen);

                                        //包头的id 就是包体的协议号
                                        int packetCode = header.Id;
                                        
                                        //获取客户端到服务器的包类型
                                        Type packType = GetClientToServerPacketType(packetCode); 
                                        
                                        if (packType == null)
                                        {
                                             Log.Error($"服务器:不能反序列化数据包 {packetCode}");
                                        }
                                        else
                                        {
                                             //通过协议号 得到包体的类型
                                             Packet packet = (Packet)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(m_ReceiveState,ReferencePool.Acquire(packType),packType,PrefixStyle.Base128,0);

                                             if (packet == null)
                                             {
                                                  Log.Error($"服务器:反序列化数据包失败 {packetCode}");
                                             }
                                             else
                                             {
                                                  //通过 处理者分发消息
                                                  m_PackDispatche.Fire(this,packet);   //GameEntry.Event.Fire(this,packet); GF案例 我们因为得不到 EventPool 临时写一个处理者
                                             }
                                        }

                                        //-----------------------------------------------------
                                        //处理包太长了 不止一个包的情况
                                        //-----------------------------------------------------

                                        //流的长度 - 全包长 = 剩余字节了
                                        int remainLen = (int)m_ReceiveState.Length - fullPacketLen;
                                        
                                        if (remainLen > 0)
                                        {
                                             //把流的位置 设置到接收数据的最后面
                                             m_ReceiveState.Position = fullPacketLen;

                                             //剩余字节缓存器
                                             byte[] remainLenBuffer = new byte[remainLen];

                                             //把剩余的字节数据读取到缓存器
                                             m_ReceiveState.Read(remainLenBuffer, 0, remainLen);
                                             
                                             //重置接收流
                                             m_ReceiveState.SetLength(0);
                                             m_ReceiveState.Position = 0;

                                             //把剩余的byte[] 数据写入到接收流头部
                                             m_ReceiveState.Write(remainLenBuffer, 0, remainLen);

                                             remainLenBuffer = null;
                                             break;
                                        }
                                        else
                                        {
                                             m_ReceiveState.SetLength(0);
                                             m_ReceiveState.Position = 0;
                                             break;
                                        }
                                   }
                                   else
                                   {
                                        //没有收到完整包 继续收
                                        break;
                                   }
                                   
                              }
                         }
                    }

                    //只要数据 > 0 就可以一直收
                    ReceiveMessage();
               }
               else
               {
                    Log.Debug($"服务器:接收客户端发送不存在信息{m_ClientSocket.RemoteEndPoint}断开连接");
               }
          }
          catch (Exception e)
          {
               Log.Debug($"服务器:捕抓客户端:{m_ClientSocket.RemoteEndPoint}接收异常断开连接 {e}");
          }
     }

     private void OnCheckSendQuene() //核查是否存在可发送消息
     {
          lock (m_SendQuene)
          {
               if (m_SendQuene.Count > 0)
               {
                    Send(m_SendQuene.Dequeue());
               }
          }
     }

     /* 测试服务器发送给客户端代码
     private void Update()
     {
          if (Input.GetKeyDown(KeyCode.C))
          {
               SCLogin login = ReferencePool.Acquire<SCLogin>();
               login.IsCanLogin = false;
               Send(login);
          }
          
          if (Input.GetKeyDown(KeyCode.D))
          {
               Send(ReferencePool.Acquire<SCHeartBeat>());
          }
     }
     */

     public bool Send<T>(T packet) where  T : Packet
     {
          PacketBase packetImpl = packet as PacketBase;
          if (packetImpl == null)
          {
               Log.Warning("服务器:Packet is invalid.");
               return false;
          }

          if (packetImpl.PacketType != PacketType.ServerToClient)
          {
               Log.Warning("服务器:Send packet invalid.");
               return false;
          }
          
          //TODO这里可以做一系列的处理
                
          //数据包压缩
                
          //数据包crc32验证
                
          //数据包加密验证 
            
          //防止无法被拷贝进入其他流
          //m_SendState.SetLength(m_SendState.Capacity);

          //序列化包体 包体可以用 PrefixStyle.Base128 压缩体积
          m_SendState.Position = PacketHeaderLength;
          Serializer.SerializeWithLengthPrefix(m_SendState, packet, PrefixStyle.Base128);

          //包头信息 包头的Id=包体的协议   包头的PacketLength=包体的byte[]长度   流的长度减去包头的长度就是真正的包长度了
          SCPacketHeader packetHeader = ReferencePool.Acquire<SCPacketHeader>();
          packetHeader.Id = packet.Id;
          packetHeader.PacketLength = (int)m_SendState.Length - PacketHeaderLength; 

          //序列化包头
          m_SendState.Position = 0;
          Serializer.SerializeWithLengthPrefix(m_SendState,packetHeader,PrefixStyle.Fixed32);
          
          //Debug.Log($"服务器: 发送消息{packet.GetType().Name} 序列化包头长度{PacketHeaderLength}  序列化包体长度{packetHeader.PacketLength}  流全长{m_SendState.Length}");
          
          ReferencePool.Release((IReference)packet);
          ReferencePool.Release(packetHeader);
          
          //发送消息
          SendMessage(m_SendState);

          //重置发送状态
          m_SendState.Position = 0L;
          m_SendState.SetLength(0L);
          return true;
     }

     private void SendMessage(MemoryStream sendStream)
     {
          m_ClientSocket.BeginSend(sendStream.GetBuffer(), 0, (int)sendStream.Length, SocketFlags.None, OnSendCallback, m_ClientSocket);
     }

     private void OnSendCallback(IAsyncResult ar)
     {
          Socket socket = (Socket)ar.AsyncState;
          if (!socket.Connected)
          {
               return;
          }

          //发送成功会返回 发送的字节数
          try
          {
               socket.EndSend(ar);
               
               //继续核查是否有可以发送的数据
               m_CheckSendQuene();
          }
          catch (Exception e)
          {
               Debug.Log($"服务器:发送消息失败{e}");
          }
     }
     
     //获取客户端-服务器的包类型
     private Type GetClientToServerPacketType(int id)
     {
          Type type = null;
          if (m_ClientToServerPacketTypes.TryGetValue(id, out type))
          {
               return type;
          }
          return null;
     }
}