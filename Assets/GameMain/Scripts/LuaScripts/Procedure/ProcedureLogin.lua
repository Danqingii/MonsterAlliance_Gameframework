
ProcedureLogin = {}

local this = ProcedureLogin
local AssetUtility = Game.AssetUtility

this.OnEnter = nil
this.OnUpdate = nil
this.OnLeave = nil

function ProcedureLogin.OnEnter(self)
    print("ProcedureLogin 流程进入")
    GameEntry.UI:OpenUIForm(AssetUtility.GetUIFormAsset("LoginForm"),"Form");
end

function ProcedureLogin.OnUpdate(self,elapseSeconds,realElapseSeconds)
    
end

function ProcedureLogin.OnLeave(self)
    print("ProcedureLogin 流程退出")
end