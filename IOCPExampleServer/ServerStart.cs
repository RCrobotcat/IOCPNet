using PENet;

// .net core 控制台服务端 .net core console server

namespace IOCPExampleServer
{
    class ServerStart
    {
        static void Main(string[] args)
        {
            /*Console.WriteLine("Hello, World!");
            IOCPTool.ColorLogFunc = (color, str) =>
            {
                Console.WriteLine($"Console: {str}");
            };
            IOCPTool.ColorLog(IOCPLogColor.Green, "Server Start...");*/
            // Testing Serialization and Deserialization
            /*IOCPMessage msg = new IOCPMessage
            {
                helloMessage = "Welcome to IOCP Server!"
            };
            byte[] data = IOCPTool.Serialize(msg);
            IOCPMessage _msg = IOCPTool.Deserialize(data);*/
            IOCPServer server = new IOCPServer();
            server.StartAsyncServer("127.0.0.1", 18000, 10000);

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "quit")
                {
                    server.CloseServer();
                }
                else
                {
                    List<IOCPToken> tokenList = server.GetTokenList();
                    for (int i = 0; i < tokenList.Count; i++)
                    {
                        tokenList[i].SendMsg(new IOCPMessage
                        {
                            helloMessage = string.Format("broadcast: {0}", input)
                        });
                    }
                }
            }
        }
    }
}
