using System;
using System.Collections.Generic;

namespace NewServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerManager.Instance.Init("127.0.0.1", 17779);
            LoginDBModel.Instance.Init();

            Console.ReadKey();
        }
    }
}