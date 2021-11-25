using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// 角色管理器
/// </summary>
public class RoleManager
{
    private static RoleManager instance;
    private static object lock_Object = new object();

    public static RoleManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lock_Object)
                {
                    if(instance == null)
                    {
                        instance = new RoleManager();
                    }
                }
            }
            return instance;
        }
    }


    private Dictionary<string,Role> m_RoleDic;


    public Dictionary<string, Role> Roles
    {
        get 
        { 
            return m_RoleDic;
        }
    }

    private RoleManager()
    {
        m_RoleDic = new Dictionary<string,Role>();
    }

    public void RegisterRole(string address, Role role)
    {
        if (!m_RoleDic.ContainsKey(address))
        {
            m_RoleDic.Add(address, role);
        }
        else
        {
            m_RoleDic[address] = role;
        }
    }

    public void UnRegisterRole(string address)
    {
        if (m_RoleDic.ContainsKey(address))
        {
            m_RoleDic.Remove(address);
        }
        else
        {
            Log.Debug($"不可用地址:{address}");
        }
    }
}