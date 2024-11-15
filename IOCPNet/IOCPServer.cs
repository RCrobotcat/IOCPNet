using System;
using System.Net;
using System.Net.Sockets;

// 基于IOCP封装的异步套接字通信的服务端实现 IOCP-based encapsulated asynchronous socket communication server implementation

namespace PENet
{
    public class IOCPServer
    {
        Socket skt;
        SocketAsyncEventArgs saea;

        public int backlog = 100; // 最大挂起连接数(最大连接排队数)

        public IOCPServer()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void StartAsServer(string ip, int port, int maxConnectionCount) // maxConnectionCount: 最大负载连接数
        {
            IPEndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            skt.Bind(pt);
            skt.Listen(backlog);

            IOCPTool.ColorLog(IOCPLogColor.Green, "Server Start...");
            StartAccept();
        }

        void StartAccept()
        {
            bool suspend = skt.AcceptAsync(saea);
            if (!suspend) // Acceptence Success.
            {
                IOCPTool.Log("Acceptance Success.");
                ProcessAccept();
            }
        }

        void ProcessAccept()
        {
            // TODO
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessAccept();
        }
    }
}
