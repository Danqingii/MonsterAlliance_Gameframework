----
---连接我们的账号服务器
---直接刷新

ProcedureLogin = {}

local this = ProcedureLogin
local AssetUtility = Game.AssetUtility

this.OnEnter = nil
this.OnUpdate = nil
this.OnLeave = nil
this.LoginChannel = nil --登陆频道

function ProcedureLogin.OnEnter(self)
    print("ProcedureLogin 流程进入")
    --GameEntry.UI:OpenUIForm(AssetUtility.GetUIFormAsset("LoginForm"),"Form");
   
    LoginChannel = GameEntry.Network:CreateChannelAndConnect("LoginChannel",0,"127.0.0.1",18889)
    
end

function ProcedureLogin.OnUpdate(self,elapseSeconds,realElapseSeconds)
    
end

function ProcedureLogin.OnLeave(self)
    print("ProcedureLogin 流程退出")
    LoginChannel:Close()
    LoginChannel = nil
end