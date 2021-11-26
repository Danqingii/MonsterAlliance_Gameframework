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

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            helper = new NetworkChannelHelper();

            INetworkChannel channel = GameEntry.Network.CreateNetworkChannel("Test", ServiceType.Tcp, helper);
            channel.Connect(IPAddress.Parse("127.0.0.1"),17779);
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (Input.GetKeyDown(KeyCode.A))
            {
                helper.SendHeartBeat();
            }
        }
    }
}