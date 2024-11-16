using System;

// 网络数据协议消息体 IOCP Message Body

namespace PENet
{
    [Serializable] // 序列化: 转化为字节数组，用于网络传输 Serialization: Convert to byte array for network transmission
    public class IOCPMessage
    {
        public string helloMessage;
    }
}
