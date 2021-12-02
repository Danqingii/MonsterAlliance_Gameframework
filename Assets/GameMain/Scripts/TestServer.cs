using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Game;
using GameFramework;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;
using UnityGameFramework.Runtime;

public class TestServer : MonoBehaviour
{
     private Socket m_Socket;

     private Thread m_ReceiveTherad;

     private byte[] m_ReceiveBuffer;           //接收数据包的字节缓冲数据流
     private MemoryStream m_ReceiveState;

     private void Awake()
     {
          m_ReceiveBuffer = new byte[8092];
          m_ReceiveState = new MemoryStream();
          Init("127.0.0.1", 17779);
     }

     public void Init(string ip,int psrt)
     {
          Log.Debug("Open Server");

          m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          m_Socket.Bind(new IPEndPoint(IPAddress.Parse(ip), psrt));
          m_Socket.Listen(100);

          Thread thread = new Thread(ListenClientCallBack); //通过线程 服务器跟客户端的连接
          thread.IsBackground = true;
          thread.Start();
     }
     
     private void ListenClientCallBack()
     {
          while (true)
          {
               //接收客户端请求
               Socket socket = m_Socket.Accept();
               Log.Debug($"客户端:{socket.RemoteEndPoint.ToString()}已经连接");

               ParameterizedThreadStart pr = new ParameterizedThreadStart(ReceiveCallback);
               m_ReceiveTherad = new Thread(pr);
               m_ReceiveTherad.IsBackground = true;
               m_ReceiveTherad.Start();
          }
     }
     
     private void ReceiveMessage()
     {
          m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallback, m_Socket);
     }
     
     /// <summary>
     /// 接收客户端发来的信息，客户端套接字对象
     /// </summary>
     private void Receive (IAsyncResult ar)
     {
          Socket socketServer = ar.AsyncState as  Socket;
          
          Debug.Log("客户的开始接受消息");
          
          while (true)
          {
               //创建一个内存缓冲区，其大小为1024*1024字节  即1M     
               byte[] arrServerRecMsg = new byte[1024 * 1024];
               //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度    
               try
               {
                    int length = socketServer.Receive(arrServerRecMsg);
                    
                    Debug.Log(length);

                    // 这里只是演示用，实际中可以根据头部消息判断是什么类型的消息，然后再反序列化
                    MemoryStream clientStream = new MemoryStream(arrServerRecMsg);
                    CSPacketHeader header = Serializer.DeserializeWithLengthPrefix<CSPacketHeader>(clientStream, PrefixStyle.Fixed32);
                    Debug.Log(header.Id + "  " + header.PacketLength);

                    //解包
                    Type packetType = typeof(CSLogin);
                    CSLogin packet = (CSLogin)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(
                         clientStream, ReferencePool.Acquire(packetType), packetType, PrefixStyle.Fixed32, 0);
                    Log.Info($"收到客户端消息:{packet.Account} {packet.Password}");

                
               }
               catch (Exception e)
               {
                    Log.Debug($"接收客户端发送不存在信息{m_Socket.RemoteEndPoint}断开连接");
               }
          }
     }

     private void ReceiveCallback(object obj)
     {
          Debug.Log("接受回调");
          
          Socket socket = obj as Socket;
          
          while (true)
          {
               byte[] arrServerRecMsg = new byte[1024 * 1024];
               //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度    
               try
               {
                    int length = socket.Receive(arrServerRecMsg);
                         
                    // 这里只是演示用，实际中可以根据头部消息判断是什么类型的消息，然后再反序列化
                    MemoryStream clientStream = new MemoryStream(arrServerRecMsg);
                    CSPacketHeader header = Serializer.DeserializeWithLengthPrefix<CSPacketHeader>(clientStream, PrefixStyle.Fixed32);
                    Debug.Log(header.Id + "  " + header.PacketLength);

                    //解包
                    Type packetType = typeof(CSLogin);
                    CSLogin packet = (CSLogin)RuntimeTypeModel.Default.DeserializeWithLengthPrefix(
                         clientStream, ReferencePool.Acquire(packetType), packetType, PrefixStyle.Fixed32, 0);
                    Log.Info($"收到客户端消息:{packet.Account} {packet.Password}");
               }
               catch (Exception e)
               {
                    Log.Debug($"捕抓客户端:{m_Socket.RemoteEndPoint}接收异常断开连接 {e}");
               }
          }
     }
}
