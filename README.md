# IOCPNet
IOCP network library implementation.  
(For learning)

## IOCPNet
IOCP网络库的应用。  
(学习用途)

# Project's Core Structure
*Core Classes*: Initiating connection, accepting connection  
*Connection Pool*: Batch creation of connections, recycling connections  
*Connection*: Data transmission and reception management  
*Tools*: Logging display, serialization and deserialization, etc.  

## 项目核心架构
*核心类*：发起连接、接受连接  
*连接池*：批量创建连接、回收连接  
*连接*：数据收发管理  
*工具*：日志显示、序列化与反序列化等  

# IOCP 简介
IOCPNet（Input/Output Completion Ports Network）是一个用于高效处理网络通信的技术架构，通常用于Windows操作系统中的网络编程。它是一种基于事件驱动的异步I/O模型，通常用于高性能、高并发的网络应用程序中。
IOCP（Input/Output Completion Port）是Windows操作系统提供的一种机制，旨在处理大量并发的网络I/O请求。它允许应用程序以异步方式进行网络通信，避免了传统的阻塞式I/O操作，因此能够提高系统的吞吐量和响应速度。
具体来说，IOCPNet是一种网络应用架构，通过IOCP机制管理大量并发连接的输入输出操作。IOCP的工作原理是，当I/O操作完成时，操作系统将相应的通知发送到应用程序的队列中，应用程序可以在不阻塞的情况下处理这些I/O请求。这使得应用程序能够高效地处理大量并发的客户端请求，常见于高并发的Web服务器、数据库服务器、游戏服务器等场景。  
  
IOCPNet的优点包括：
1. **高并发支持**：能够处理大量并发连接，尤其适合于高吞吐量的网络应用。  
2. **异步I/O**：避免了阻塞I/O操作，提高了程序的响应性和效率。  
3. **低资源消耗**：与传统的多线程或多进程模型相比，IOCP能够以较少的系统资源管理大量并发连接。  
  
IOCP通常与套接字编程结合使用，是Windows平台上开发高性能网络服务的一个重要工具。

## Unity中使用示例:
**UnityToken.cs**  
```
using PENet;
using IOCPExampleProtocol;

public class UnityToken : IOCPToken<NetMessage>
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
```

**TestStart.cs**  
```
using UnityEngine;
using PENet;
using IOCPExampleProtocol;

// Unity IOCP 客户端入口
// Unity IOCP Client Entry

public class TestStart : MonoBehaviour
{
    IOCPNet<UnityToken, NetMessage> client;

    void Start()
    {
        client = new IOCPNet<UnityToken, NetMessage>();
        client.StartAsyncClient("127.0.0.1", 18000);

        IOCPTool.LogFunc = Debug.Log;
        IOCPTool.WarnFunc = Debug.LogWarning;
        IOCPTool.ErrorFunc = Debug.LogError;
        IOCPTool.ColorLogFunc = (color, message) =>
        {
            Debug.Log(color.ToString() + ": " + message);
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NetMessage msg = new NetMessage
            {
                helloMessage = "Hello from Unity!"
            };
            client.token.SendMsg(msg);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            client.CloseClient();
        }
    }
}
```
