using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ServerManager
{
    private static byte[] result = new byte[1024];
    public Dictionary<string, Socket> clients = new Dictionary<string, Socket>();
    private static ServerManager serverManager;

    public static ServerManager Instance
    {
        get
        {
            if(serverManager == null)
            {
                serverManager = new ServerManager();
            }

            return serverManager;
        }
    }

    public Socket serverSocket
    {
        get;
        private set;
    }

    public void Init(string ip,int psrt)
    {
        Log.Debug("Open Server");

        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), psrt));
        serverSocket.Listen(100);

        Log.Debug($"启动监听{serverSocket.LocalEndPoint.ToString()}成功");

        Thread thread = new Thread(ListenClientCallBack); //通过线程 服务器跟客户端的连接
        thread.IsBackground = true;
        thread.Start();
    }

    //等待连接客户端
    private void ListenClientCallBack()
    {
        while (true)
        {
            //接收客户端请求
            Socket socket = serverSocket.Accept();
            Log.Debug($"客户端:{socket.RemoteEndPoint.ToString()}已经连接");

            //一个角色就相当于一个客户端
            Role role = new Role();
            ClientSocket clientSocket = new ClientSocket(role, socket);
            role.SetClientSocket(clientSocket);

            RoleManager.Instance.RegisterRole(socket.RemoteEndPoint.ToString(), role);
        }
    }
}
