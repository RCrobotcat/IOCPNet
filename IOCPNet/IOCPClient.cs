using System;
using System.Net;
using System.Net.Sockets;

// 基于IOCP封装的异步套接字通信 IOCP-based encapsulated asynchronous socket communication

namespace PENet
{
    public class IOCPClient
    {
        Socket skt;
        SocketAsyncEventArgs saea;

        public IOCPClient()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void StartAsClient(string ip, int port)
        {
            IPEndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            saea.RemoteEndPoint = pt; // Set the remote endpoint for socket.

            IOCPTool.ColorLog(IOCPLogColor.Green, "Client Start...");
            StartConnect();
        }


        /// <summary>
        /// 1. 异步连接事件被挂起: 连接没有建立成功，要等到连接建立成功后再创建管理类，开始数据收发
        /// 2. 异步连接事件没有被挂起: 连接建立成功，创建连接管理类，开始数据收发
        /// </summary>
        void StartConnect()
        {
            bool suspend = skt.ConnectAsync(saea);
            if (!suspend) // Connection Success.
            {
                IOCPTool.Log("Connection Success.");
                ProcessConnect();
            }
            else
            {
                IOCPTool.Log("Connection Pending..."); // 连接挂起
            }
        }

        void ProcessConnect()
        {
            // TODO
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessConnect();
        }
    }
}
