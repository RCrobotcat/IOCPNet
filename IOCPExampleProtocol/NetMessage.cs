using PENet;
using System;

// 网络数据协议消息体 Network data protocol message body

namespace IOCPExampleProtocol
{
    [Serializable]
    public class NetMessage : IOCPMessage
    {
        public string helloMessage;
    }
}
