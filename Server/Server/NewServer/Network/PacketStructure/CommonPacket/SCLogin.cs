using System;
using ProtoBuf;

public class SCLogin : SCPacketBase
{
    public SCLogin()
    {
    }

    public override int Id
    {
        get
        {
            return 20001;
        }
    }

    public bool IsCanLogin
    {
        get;
        set;
    }

    public override void Clear()
    {
    }
}