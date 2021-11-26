using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// 客户端连接对象  负责和客户端进行通讯的
/// </summary>
public class ClientSocket
{
    private Role m_Role;
    private Socket m_Socket;

    private Thread m_ReceiveTherad;

    private CustomMemoryStream m_ReceiveMS;   //接收数据包的字节数组缓冲区
    private byte[] m_ReceiveBuffer;           //接收数据包的字节缓冲数据流

    private const int m_CompressLen = 200;                            //压缩的字节长度
    private readonly Queue<byte[]> m_SendQuene = new Queue<byte[]>(); //发送消息队列
    private Action m_CheckSendQuene;                                  //核查队列的委托


    public ClientSocket(Role role,Socket socket)
    {
        m_Role = role;
        m_Socket = socket;
        m_ReceiveBuffer = new byte[1024 * 4];
        m_ReceiveMS = new CustomMemoryStream();

        //启动线程 进行接收数据
        m_ReceiveTherad = new Thread(ReceiveMessage);
        m_ReceiveTherad.IsBackground = true;
        m_ReceiveTherad.Start();


        m_CheckSendQuene = OnCheckSendQueneCallBack;
    }

    #region 接收消息
    //接收数据
    private void ReceiveMessage()
    {
        //开始异步接收数据
        m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCallBack, m_Socket);
    }

    private void ReceiveCallBack(IAsyncResult ar)
    {
        int len = m_Socket.EndReceive(ar);

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

                //TODO
                Log.Debug(len.ToString());

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
                            //定义一个包体的byte[]数组
                            byte[] buffer = new byte[curMsgLen];

                            //把数据流的指针放到2的位置  也就是包体的位置
                            m_ReceiveMS.Position = 2;

                            //把包体读到缓存区里面
                            m_ReceiveMS.Read(buffer, 0, curMsgLen);

                            //================================================
                            //解包 分发 一个过程
                            byte[] newBuffer = new byte[buffer.Length - 3];

                            //是否压缩过
                            bool isCompress = false;

                            //得到传过来的验证码
                            ushort crc16 = 0;

                            using (CustomMemoryStream ms = new CustomMemoryStream(buffer))
                            {
                                isCompress = ms.ReadBool();
                                crc16 = ms.ReadUShort();
                                ms.Read(newBuffer, 0, newBuffer.Length);
                            }
  
                            //进行crc校验
                            int newCrc16 = Crc16.CalcCrc16(newBuffer);
                            //校验通过 可以继续拆包
                            if (newCrc16 == crc16)
                            {
                                //异或 得到原始数据
                                newBuffer = SecurityUtil.Xor(newBuffer);

                                //如果被压缩过了 就解压缩了可以了
                                if (isCompress)
                                {
                                    newBuffer = CompressionHelper.Decompress(newBuffer);
                                }

                                //=========================================
                                //经过了上面的 校验crc 异或解密 解压缩  得到了真正的数据包 = newBuffer
                                //=========================================

                                //协议编号
                                ushort protoCode = 0;

                                //具体的协议内容
                                byte[] protoContent = new byte[newBuffer.Length - 2];

                                //解析真正的数据包 得到协议 跟协议内容 分发消息
                                using (CustomMemoryStream ms = new CustomMemoryStream(newBuffer))
                                {
                                    protoCode = ms.ReadUShort();
                                    ms.Read(protoContent, 0, protoContent.Length);
                                }

                                //派发消息
                                EventDispatcher.Instance.Dispatch(protoCode, m_Role, protoContent);
                            }
                            //经过了上面的过程  处理了一个真正数据包
                            //================================================


                            //================================================================
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
                        else
                        {
                            //还没有收到完整包
                            break;
                        }
                    }
                }

                //循环接收数据包 一个包接完了  可以继续接收了
                ReceiveMessage();
            }
            else
            {
                Log.Debug($"客户端:{m_Socket.RemoteEndPoint.ToString()}断开连接");
                RoleManager.Instance.UnRegisterRole(m_Socket.RemoteEndPoint.ToString());
            }
        }
        catch (Exception e)
        {
            Log.Debug($"客户端:{m_Socket.RemoteEndPoint.ToString()}断开连接");
            RoleManager.Instance.UnRegisterRole(m_Socket.RemoteEndPoint.ToString());
        }
    }
    #endregion

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
        m_Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, OnSendCallBack, m_Socket);
    }

    //发送数据包回调
    private void OnSendCallBack(IAsyncResult ar)
    {
        m_Socket.EndSend(ar);

        //继续检查队列
        m_CheckSendQuene();
    }

    //发送消息 只是把消息加入队列而已
    public void SendMsg(byte[] buffer)
    {
        //得到封装后的数据包
        byte[] sendBuffer = MakeData(buffer);
        lock (m_SendQuene)
        {
            //把数据包加入队列
            m_SendQuene.Enqueue(sendBuffer);

            //执行委托
            m_CheckSendQuene.BeginInvoke(null, null);
        }
    }

    //封装数据包
    private byte[] MakeData(byte[] data)
    {
        byte[] buffer = null;

        //是否压缩
        bool isCompress = data.Length >= m_CompressLen;
        if (isCompress)
        {
            data = CompressionHelper.Compress(data);
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
            ms.Write(data, 0, data.Length);        //具体的数据包写入
            buffer = ms.ToArray();
        }

        return buffer;
    }
    #endregion
}