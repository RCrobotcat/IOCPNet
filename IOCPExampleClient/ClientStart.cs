using PENet;
using System;

// .Net Framework控制台客户端 .Net Framework Console Client

namespace IOCPExampleClient
{
    class ClientStart
    {
        static void Main(string[] args)
        {
            IOCPClient client = new IOCPClient();
            client.StartAsyncClient("127.0.0.1", 18000); // port: 0-65535
            Console.ReadKey();
        }
    }
}
