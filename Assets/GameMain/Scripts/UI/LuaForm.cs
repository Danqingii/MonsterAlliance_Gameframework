using UnityEngine;
using UnityGameFramework.Runtime;
using XLua;

namespace Game
{
    public class LuaForm : UGuiForm
    {
        //初始化的时候 利用该名字 指向Lua脚本进行轮询
        [SerializeField] private string m_FormName;
        
        private LuaTable m_GlobalFormMgr;      //Lua全局管理表
        private LuaTable m_FormToLuaNameTable; //Lua的名字对应表
        private LuaTable m_FormClassTable;     //Lua的具体类表

        private bool m_Executable;      //是否是Lua可执行的生命周期
        private LuaFunction m_Init;     //初始化时
        private LuaFunction m_Recycle;  //回收利用时
        private LuaFunction m_Open;     //打开时
        private LuaFunction m_Close;    //关闭时
        private LuaFunction m_Pause;    //暂停时
        private LuaFunction m_Resume;   //恢复时
        private LuaFunction m_Reveal;   //揭示时--没看懂
        private LuaFunction m_Refocus;  //重新聚焦--没看懂
        private LuaFunction m_Update;   //轮询时
        private LuaFunction m_Cover;    //覆盖时
        private LuaFunction m_DepthChanged;  //层级改变时
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            m_GlobalFormMgr = GameEntry.Lua.GetGlobalLuaTable("UI/LuaFormManager", "LuaFormManager");
            m_FormToLuaNameTable = GameEntry.Lua.GetChildTable(m_GlobalFormMgr ,"FormToLuaNames");
            
            if (m_GlobalFormMgr != null)
            {
                string luaScriptName = m_FormToLuaNameTable.Get<string>(m_FormName);
                GameEntry.Lua.DoScript(luaScriptName,"LuaForm");
                
                LuaTable dictTable = GameEntry.Lua.GetChildTable(m_GlobalFormMgr, "FormClassDict");
                m_FormClassTable = dictTable.Get<LuaTable>(m_FormName);

                if (m_FormClassTable == null)
                {
                    m_Executable = false;
                    Log.Error($"LuaForm{m_FormName} 执行脚本 获取失败");
                    return;
                }

                m_FormClassTable.Get("OnInit",out m_Init);
                m_FormClassTable.Get("OnRecycle",out m_Recycle);
                m_FormClassTable.Get("OnOpen",out m_Open);
                m_FormClassTable.Get("OnClose",out m_Close);
                m_FormClassTable.Get("OnPause",out m_Pause);
                m_FormClassTable.Get("OnResume",out m_Resume);
                m_FormClassTable.Get("OnReveal",out m_Reveal);
                m_FormClassTable.Get("OnRefocus",out m_Refocus);
                m_FormClassTable.Get("OnUpdate",out m_Update);
                m_FormClassTable.Get("OnCover",out m_Cover);
                m_FormClassTable.Get("OnDepthChanged",out m_DepthChanged);
                m_Executable = true;

                if (m_Executable)
                {
                    m_Init?.Call(this);
                }
            }
        }

        protected override void OnRecycle()
        {
            base.OnRecycle();
            if (m_Executable)
            {
                m_Recycle?.Call();
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_Executable)
            {
                m_Open?.Call(userData);
            }
        }
        
        protected override void OnClose(bool isShutdown,object userData)
        {
            base.OnClose(isShutdown,userData);
            if (m_Executable)
            {
                m_Close?.Call(isShutdown);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (m_Executable)
            {
                m_Pause?.Call();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (m_Executable)
            {
                m_Resume?.Call();
            }
        }

        protected override void OnReveal()
        {
            base.OnReveal();
            if (m_Executable)
            {
                m_Reveal?.Call();
            }
        }

        protected override void OnRefocus(object userData)
        {
            base.OnRefocus(userData);
            if (m_Executable)
            {
                m_Refocus?.Call();
            }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (m_Executable)
            {
                m_Update?.Call(elapseSeconds,realElapseSeconds);
            }
        }

        protected override void OnCover()
        {
            base.OnCover();
            if (m_Executable)
            {
                m_Cover?.Call();
            }
        }

        protected override void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            base.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            if (m_Executable)
            {
                m_DepthChanged?.Call(uiGroupDepth,depthInUIGroup);
            }
        }
    }
}
