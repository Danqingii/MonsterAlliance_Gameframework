using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using GameFramework;
using GameFramework.Event;
using GameFramework.Network;
using ProtoBuf;
using UnityEngine;
using UnityGameFramework.Runtime;
using AddressFamily = System.Net.Sockets.AddressFamily;

namespace Game
{
    public class TcpNetworkComponent : GameFrameworkComponent
    {
        [SerializeField] private byte[] m_Ip = new byte[4];
        [SerializeField] private int m_Port;
        [SerializeField] private string m_TcpName = "TcpNetwork";

        private Socket m_ClientSocket;
        
        private const int m_CompressLen = 200;
        private readonly Queue<byte[]> m_SendQuene = new Queue<byte[]>(); //发送消息队列
        private Action m_CheckSendQuene;                                  //核查队列的委托
        
        private CustomMemoryStream m_ReceiveMS = new CustomMemoryStream();//接收数据包的字节数组缓冲区
        private byte[] m_ReceiveBuffer = new byte[4096];                  //接收消息缓冲区
        private readonly Queue<byte[]> m_ReceiveQuene = new Queue<byte[]>();//接收消息队列
        private int m_ReceiveCount = 0;                                     //一帧接收的数量
        
        public void StartConnect()
        {
            try
            {
                m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //创建套接字
                IPAddress ipAddress = new IPAddress(m_Ip);
                IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, m_Port);
                m_ClientSocket.Connect(ipEndpoint);

                //开始监听发送消息
                lock (m_SendQuene)
                {
                    m_CheckSendQuene = OnCheckSendQueneCallBack;
                }

                //开始监听接收消息
                ReceiveMessage();
            }
            catch (GameFrameworkException e)
            {
                Debug.LogError($"连接失败,错误: {e}.");
                ShutDown();
            }
        }

        private void OnDestroy()
        {
            ShutDown();
        }

        public void ShutDown()
        {
            if (m_ClientSocket != null && m_ClientSocket.Connected)
            {
                m_ClientSocket.Shutdown(SocketShutdown.Both);
                m_ClientSocket.Close();
            }
        }

        private void Update()
        {
            while (true)
            {
                if (m_ReceiveCount <= 5)
                {
                    m_ReceiveCount += 1;
                    lock (m_ReceiveQuene)
                    {
                        if (m_ReceiveQuene.Count > 0)
                        {
                            //队列中的数据包
                            byte[] buffer = m_ReceiveQuene.Dequeue();

                            //解包 
                            byte[] newBuffer = new byte[buffer.Length - 3];

                            bool isCompress = false;
                            ushort crc16 = 0;

                            using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
                            {
                                isCompress = ms.ReadBool();
                                crc16 = ms.ReadUShort();
                                ms.Read(newBuffer,0,newBuffer.Length);
                            }
                            
                            //进行crc校验
                            int newCrc16 = Crc16.CalcCrc16(newBuffer);
                            //校验通过 
                            if (newCrc16 == crc16)
                            {
                                //异或 得到原始数据
                                newBuffer = SecurityUtil.Xor(newBuffer);
                            
                                //如果被压缩过了 就解压缩了可以了
                                if (isCompress)
                                {
                                    newBuffer = Utility.Compression.Decompress(newBuffer);
                                }

                                ushort protoCode = 0;
                                byte[] protoContent = new byte[newBuffer.Length - 2];
                                //到达了这一步才是我们真正想要的数据
                                using (CustomMemoryStream ms = new CustomMemoryStream(newBuffer))
                                {
                                    protoCode = ms.ReadUShort();
                                    ms.Read(protoContent, 0, protoContent.Length);
                                }

                                //转发协议
                                EventDispatcher.Instance.Dispatch(protoCode,protoContent);
                            }
                            else
                            {
                                //Crc 验证失败
                                break;
                            }
                        }
                        else
                        {
                            //没有需要接收的数量
                            break;
                        }
                    }
                }
                else
                {
                    //如果没有接到5个数量 就跳出
                    m_ReceiveCount = 0;
                    break;
                }
            }
        }

        #region 发送消息
        //检查队列的委托回调
        private void OnCheckSendQueneCallBack()
        {
            lock (m_SendQuene)
            {
                //如果队列中有数据包就可以发送了
                if (m_SendQuene.Count > 0)
                {
                    Send(m_SendQuene.Dequeue());
                }
            }
        }

        //真正的发送消息
        private void Send(byte[] buffer)
        {
            m_ClientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None,OnSendCallBack,m_ClientSocket);
        }

        //发送数据包回调
        private void OnSendCallBack(IAsyncResult ar)
        {
            m_ClientSocket.EndSend(ar);

            //继续检查队列
            m_CheckSendQuene();
        }

        //发送消息 只是把消息加入队列而已
        public void SendMsg(byte[] buffer)
        {
            //得到封装后的数据包
            byte[] packet = MakeData(buffer);
            lock (m_SendQuene)
            {
                //把数据包加入队列
                m_SendQuene.Enqueue(packet);

                //执行委托
                m_CheckSendQuene.BeginInvoke(null, null);
            }
        }

        //封装数据包
        public byte[] MakeData(byte[] data)
        {
            byte[] buffer = null;
            
            //是否压缩
            bool isCompress = data.Length >= m_CompressLen;
            if (isCompress)
            {
                data = Utility.Compression.Compress(data);
            }
            
            //然后异或 也就是加密
            data = SecurityUtil.Xor(data);
            
            //得到crc16校验码  压缩后的校验
            ushort crc16 = Crc16.CalcCrc16(data);
            
            using (CustomMemoryStream ms = new CustomMemoryStream())
            {
                //长度 + 3   是因为 bool = 1字节   ushort = 2字节
                ms.WriteUShort((ushort)(data.Length + 3));       //包体的长度
                ms.WriteBool(isCompress);                        //是否压缩
                ms.WriteUShort(crc16);                           //crc16验证码
                ms.Write(data,0,data.Length);        //具体的数据包写入
                buffer = ms.ToArray();                          
            }

            return buffer;
        }
        
        #endregion

        #region 接收消息

         //接收数据
         private void ReceiveMessage()
         {
             //开始异步接收数据
             m_ClientSocket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_ClientSocket);
         }

         private void ReceiveCallBack(IAsyncResult ar)
         {
             int len = m_ClientSocket.EndReceive(ar);

             try
             {
                 //存在数据
                 if (len > 0)
                 {
                     //把接收到的数据 写入缓冲数据流的尾部
                     m_ReceiveMS.Position = m_ReceiveMS.Length;

                     //把指定长度的字节  写入数据流
                     m_ReceiveMS.Write(m_ReceiveBuffer, 0, len);

                     //如果缓存数据流的长度大于2  至少有一个不完整的包过来了
                     //为什么是2呢  因为我们客户端数据包 ushort 长度为2
                     if(m_ReceiveMS.Length > 2)
                     {
                         //循环 拆包
                         while (true)
                         {
                             //把数据流指针位置放在0处
                             m_ReceiveMS.Position = 0;

                             //包体的长度
                             int curMsgLen = m_ReceiveMS.ReadUShort();

                             //总包的长度 = 包体长度 + 包头长度
                             int curFullMsgLen = curMsgLen + 2;

                             //如果数据流的长度 >= 整包的长度 说明至少收到了一个完整包
                             if (m_ReceiveMS.Length >= curFullMsgLen)
                             {
                                 //定义一个缓存器
                                 byte[] buffer = new byte[curFullMsgLen];

                                 //把数据流的指针放到2的位置  也就是包体的位置
                                 m_ReceiveMS.Position = 2;

                                 //把包体读到缓存区里面
                                 m_ReceiveMS.Read(buffer, 0, curFullMsgLen);

                                 lock (m_ReceiveQuene)
                                 {
                                     m_ReceiveQuene.Enqueue(buffer);
                                 }

                                 //可能发包不止发了一个
                                 //=================处理剩余字节数组===============================

                                 //剩余字节长度
                                 int remainLen = (int)m_ReceiveMS.Length - curFullMsgLen;

                                 if (remainLen > 0)
                                 {
                                     //把指针的位置放到第一个包的尾部
                                     m_ReceiveMS.Position = curFullMsgLen;

                                     //剩余包缓存器
                                     byte[] remainBuffer = new byte[remainLen];

                                     //把剩下的数据流 读到剩余字节数组
                                     m_ReceiveMS.Read(remainBuffer, 0, remainLen);

                                     //清空数据流
                                     m_ReceiveMS.Position = 0;
                                     m_ReceiveMS.SetLength(0);

                                     //把剩余的字节数组 重新写入数据流 并且
                                     m_ReceiveMS.Write(remainBuffer, 0, remainBuffer.Length);

                                     remainBuffer = null;
                                 }

                                 //没有剩余字节  没有剩余包了
                                 else
                                 {
                                     //清空数据流
                                     m_ReceiveMS.Position = 0;
                                     m_ReceiveMS.SetLength(0);
                                     break;
                                 }
                             }

                             //还没有收到完整包
                             else
                             {
                                 break;
                             }
                         }
                     }

                     //循环接收数据包 一个包接完了  可以继续接收了
                     ReceiveMessage();
                 }
                 else
                 {
                     Log.Debug($"服务端:{m_ClientSocket.RemoteEndPoint.ToString()}断开连接");
                     ShutDown();
                 }
             }
             catch (Exception e)
             {
                 Log.Debug($"服务端:{m_ClientSocket.RemoteEndPoint.ToString()}断开连接");
                 ShutDown();
             }
         }
         #endregion
    }
}