using IOCPExampleProtocol;
using PENet;

// .Net Framework客户端会话管理 .Net Framework client session management

namespace IOCPExampleClient
{
    public class ClientToken : IOCPToken<NetMessage>
    {
        protected override void OnConnected()
        {
            IOCPTool.ColorLog(IOCPLogColor.Green, "Connected to Server!");
        }

        protected override void OnDisconnected()
        {
            IOCPTool.WarnLog("Disconnected from Server!");
        }

        protected override void OnRecieveMessage(NetMessage msg)
        {
            IOCPTool.Log("Message Received from Server: {0}", msg.helloMessage);
        }
    }
}
