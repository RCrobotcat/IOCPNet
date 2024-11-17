using IOCPExampleProtocol;
using PENet;
using System;

// .Net Framework控制台客户端 .Net Framework Console Client

namespace IOCPExampleClient
{
    class ClientStart
    {
        static void Main(string[] args)
        {
            IOCPNet<ClientToken, NetMessage> client = new IOCPNet<ClientToken, NetMessage>();
            client.StartAsyncClient("127.0.0.1", 18000); // port: 0-65535

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "quit")
                {
                    client.CloseClient();
                    break;
                }
                else
                {
                    NetMessage msg = new NetMessage
                    {
                        helloMessage = input
                    };
                    client.token.SendMsg(msg);
                }
            }
        }
    }
}