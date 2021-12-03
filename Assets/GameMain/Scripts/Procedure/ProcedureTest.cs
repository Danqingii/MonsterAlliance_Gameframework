using System.Net;
using GameFramework.Fsm;
using GameFramework.Network;
using GameFramework.Procedure;
using UnityEngine;

namespace Game
{
    public class ProcedureTest:ProcedureBase
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
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.A))
            {
                //channel.Send(new CSLogin(){Account = "1", Password = "2"});
            }
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                //helper.SendHeartBeat();
            }
        }
    }
}