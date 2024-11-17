using IOCPExampleProtocol;
using PENet;

// .net core 服务端会话管理 .net core server session management

namespace IOCPExampleServer
{
    public class ServerToken : IOCPToken<NetMessage>
    {
        protected override void OnConnected()
        {
            IOCPTool.ColorLog(IOCPLogColor.Green, "Client Online!");
        }

        protected override void OnDisconnected()
        {
            IOCPTool.WarnLog("Client Offline!");
        }

        protected override void OnRecieveMessage(NetMessage msg)
        {
            IOCPTool.Log("Message Received from Client: {0}", msg.helloMessage);
        }
    }
}
