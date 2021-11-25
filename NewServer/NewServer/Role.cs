using System;
using System.Collections.Generic;
using System.Text;

public class Role
{
    public ClientSocket ClientSocket
    {
        get;
        private set;
    }

    public void SetClientSocket(ClientSocket socket)
    {
        ClientSocket = socket;
    }
}