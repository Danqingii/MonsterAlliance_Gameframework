using System.Net;
using GameFramework.Fsm;
using GameFramework.Network;
using GameFramework.Procedure;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 测试网络模块
    /// </summary>
    public class ProcedureTestNetwork:ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        private NetworkChannelHelper helper;
        private INetworkChannel channel;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            helper = new NetworkChannelHelper();
            channel = GameEntry.Network.CreateNetworkChannel("Test", ServiceType.Tcp, helper);
            channel.Connect(IPAddress.Parse("127.0.0.1"),17779);
            
            /* 测试序列化的字节数
            MemoryStream cc = new MemoryStream(1024 * 8);
            CSPacketHeader cc1 = ReferencePool.Acquire<CSPacketHeader>();
            cc1.Id = 10000000;
            cc1.PacketLength = 200000000;
            
            cc.Position = 0;
            Serializer.Serialize(cc,cc1);
            Debug.Log(cc.Length);
            */
            
            /* 测试protobuf 序列化跟反序列化
            MemoryStream snedStream = new MemoryStream(1024 * 8);
            
            CSLogin snedLogin = ReferencePool.Acquire<CSLogin>();
            snedLogin.Account = "11000000";
            snedLogin.Password = "110000";
            
            snedStream.Position = 8;
            Serializer.SerializeWithLengthPrefix(snedStream, snedLogin, PrefixStyle.Fixed32);
            
            CSPacketHeader snedHeader = ReferencePool.Acquire<CSPacketHeader>();
            snedHeader.Id = snedLogin.Id;
            snedHeader.PacketLength = 200000000;
            
            snedStream.Position = 0;
            Serializer.SerializeWithLengthPrefix(snedStream,snedHeader,PrefixStyle.Fixed32);
            
            Debug.Log($"序列化包头长度{8}  序列化包体长度{snedHeader.PacketLength}");
            
            //序列化
            ReferencePool.Release(snedHeader);
            ReferencePool.Release((IReference)snedLogin);

            byte[] sendBytes = snedStream.GetBuffer();
            int len = (int)snedStream.Length;
            //-------------------------------------------------------------------------
            
            MemoryStream receiveStream = new MemoryStream(1024 * 8);
            receiveStream.Write(sendBytes,0,len);

            receiveStream.Position = 0;
            CSPacketHeader header = Serializer.DeserializeWithLengthPrefix<CSPacketHeader>(receiveStream, PrefixStyle.Fixed32);
            
            Debug.Log($"{header.Id}  {header.PacketLength}");
            
            receiveStream.Position = 8;
            CSLogin packet = Serializer.DeserializeWithLengthPrefix<CSLogin>(receiveStream, PrefixStyle.Fixed32);
            
            Debug.Log($"{packet.Account}  {packet.Password}");
            */
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.A))
            {
                channel.Send(new CSLogin(){Account = "1", Password = "2"});
            }
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                helper.SendHeartBeat();
            }
        }
    }
}