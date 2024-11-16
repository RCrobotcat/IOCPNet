using PENet;

// 控制台服务端 Console Server

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
            IOCPMessage msg = new IOCPMessage
            {
                helloMessage = "Welcome to IOCP Server!"
            };
            byte[] data = IOCPTool.Serialize(msg);
            IOCPMessage _msg = IOCPTool.Deserialize(data);

            IOCPServer server = new IOCPServer();
            server.StartAsServer("127.0.0.1", 19000, 10000);
            Console.ReadKey();
        }
    }
}
