//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2021-11-24 15:31:27
//备    注：
//===================================================
using System.Collections;

/// <summary>
/// 协议编号定义
/// </summary>
public class ProtoCodeDef
{
    /// <summary>
    /// 客户端发送本地时间
    /// </summary>
    public const ushort System_SendLocalTime = 14001;

    /// <summary>
    /// 服务器返回服务器时间
    /// </summary>
    public const ushort System_ServerTimeReturn = 14002;

    /// <summary>
    /// 服务器返回配置列表
    /// </summary>
    public const ushort System_GameServerConfigReturn = 14003;

    /// <summary>
    /// 获取邮件详情
    /// </summary>
    public const ushort CS_Get_Datail = 14004;

    /// <summary>
    /// 测试协议
    /// </summary>
    public const ushort CS_Ret_List = 14005;

    /// <summary>
    /// 登陆协议
    /// </summary>
    public const ushort CS_Login = 10001;
    
    /// <summary>
    /// 服务器登陆协议
    /// </summary>
    public const ushort SC_Login = 10002;

}
