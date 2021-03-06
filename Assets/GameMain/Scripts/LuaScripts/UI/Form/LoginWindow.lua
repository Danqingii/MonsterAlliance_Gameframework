---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by 20200506QASD.
--- DateTime: 2022/1/12 18:45
---

LoginWindow = {}

local this = LoginWindow
LuaFormManager.FormClassDict["LoginWindow"] = this --这一步很关键把自己给注册进管理器中

local closeBtn = nil         --关闭页面
local confirmLoginBtn = nil  --确定登陆
local idInputField = nil     --账号输入
local pwInputField = nil     --密码输入
local savePwToggle = nil     --保存密码 放在本地

function LoginWindow.OnInit(self)
    
    closeBtn = self.transform:Find("Popup_Login/Popup/Button_Close"):GetComponent("Button")
    confirmLoginBtn = self.transform:Find("Popup_Login/Popup/Button_ConfirmLogin"):GetComponent("Button")
    idInputField = self.transform:Find("Popup_Login/Popup/InputFields/Input_Id"):GetComponent("TMP_InputField")
    pwInputField = self.transform:Find("Popup_Login/Popup/InputFields/Input_Pw"):GetComponent("TMP_InputField")
    savePwToggle = self.transform:Find("Popup_Login/Popup/Toggle_SavePw"):GetComponent("Toggle")

    --添加一个默认关闭的方法
    closeBtn.onClick:AddListener(function() self:Close() end)
    confirmLoginBtn.onClick:AddListener(this.ConfirmLoginClick)
end

function LoginWindow.OnOpen(cachedTransform, userData)
    print("LoginWindow 打开")
end

function LoginWindow.ConfirmLoginClick()
    
    --给服务器发一条消息 然后返回数据
end


