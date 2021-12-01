using System;


/// <summary>
/// 事件基类。
/// </summary>
public abstract class BaseEventArgs : EventArgs
{
    /// <summary>
    /// 获取类型编号。
    /// </summary>
    public abstract int Id
    {
        get;
    }

    /// <summary>
    /// 清理引用。
    /// </summary>
    public abstract void Clear();
}