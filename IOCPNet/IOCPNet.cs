using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// 基于IOCP封装的异步套接字通信的客户端和服务端实现
// IOCP-based encapsulated asynchronous socket communication Client implementation and Server implementation

namespace PENet
{
    [Serializable] // 序列化: 转化为字节数组，用于网络传输 Serialization: Convert to byte array for network transmission
    public abstract class IOCPMessage
    { }

    public class IOCPNet<T, K>
        where T : IOCPToken<K>, new()
        where K : IOCPMessage, new()
    {
        Socket skt;
        SocketAsyncEventArgs saea;

        public IOCPNet()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        #region IOCPClient
        public T token;
        public void StartAsyncClient(string ip, int port)
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
                IOCPTool.Log("Connection Success."); // 连接成功
                ProcessConnect();
            }
            else
            {
                IOCPTool.Log("Connection Pending..."); // 连接挂起
            }
        }

        void ProcessConnect()
        {
            token = new T();
            token.InitToken(skt);
        }

        public void CloseClient()
        {
            if (token != null)
            {
                token.CloseToken();
                token = null;
            }
            if (skt != null)
            {
                skt = null;
            }
        }
        #endregion

        #region IOCPServer
        int currentConnectionCount = 0; // 当前连接数
        Semaphore acceptSemaphore; // 信号量: 限制同时接受的连接数 Semaphore: Limit the number of connections accepted at the same time
        public int backlog = 100; // 最大挂起连接数(最大连接排队数)
        IOCPTokenPool<T, K> tokenPool;
        private List<T> tokenList;
        public void StartAsyncServer(string ip, int port, int maxConnectionCount) // maxConnectionCount: 最大负载连接数
        {
            currentConnectionCount = 0;
            acceptSemaphore = new Semaphore(maxConnectionCount, maxConnectionCount);
            tokenPool = new IOCPTokenPool<T, K>(maxConnectionCount);
            for (int i = 0; i < maxConnectionCount; i++)
            {
                T token = new T
                {
                    tokenID = i
                };
                tokenPool.Push(token);
            }
            tokenList = new List<T>();

            IPEndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            skt.Bind(pt);
            skt.Listen(backlog);

            IOCPTool.ColorLog(IOCPLogColor.Green, "Server Start...");
            StartAccept();
        }

        void StartAccept()
        {
            saea.AcceptSocket = null;
            acceptSemaphore.WaitOne(); // Wait for the semaphore to be released. 等待信号量被释放(信号量减1)
            bool suspend = skt.AcceptAsync(saea);
            if (!suspend) // Acceptence Success.
            {
                IOCPTool.Log("Acceptance Success.");
                ProcessAccept();
            }
        }

        void ProcessAccept()
        {
            Interlocked.Increment(ref currentConnectionCount);
            T token = tokenPool.Pop();
            if (tokenList == null)
                tokenList = new List<T>();
            lock (tokenList)
            {
                tokenList.Add(token);
            }
            token.InitToken(saea.AcceptSocket);
            token.tokenCloseCallback = OnTokenClose;
            IOCPTool.ColorLog(IOCPLogColor.Green, "Client Online, Allocate tokenID: {0}", token.tokenID);
            StartAccept();
        }

        void OnTokenClose(int tokenID)
        {
            int index = -1;
            for (int i = 0; i < tokenList.Count; i++)
            {
                if (tokenList[i].tokenID == tokenID)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1) // 下线回收Token
            {
                tokenPool.Push(tokenList[index]);
                lock (tokenList)
                {
                    tokenList.RemoveAt(index);
                }
                Interlocked.Decrement(ref currentConnectionCount);
                acceptSemaphore.Release(); // 信号量下线一个(信号量加1)
            }
            else
            {
                IOCPTool.ErrorLog("TokenID: {0} not found in server token list.", tokenID);
            }
        }

        public void CloseServer()
        {
            for (int i = 0; i < tokenList.Count; i++)
            {
                tokenList[i].CloseToken();
            }
            tokenList = null;
            if (skt != null)
            {
                skt.Close();
                skt = null;
            }
        }

        /// <summary>
        /// 获取所有的连接
        /// </summary>
        public List<T> GetTokenList()
        {
            return tokenList;
        }
        #endregion

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch (saea.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    ProcessAccept();
                    break;
                case SocketAsyncOperation.Connect:
                    ProcessConnect();
                    break;
                default:
                    IOCPTool.WarnLog("The last operation completed on the socket was not a accept or connect operation.");
                    break;
            }
        }
    }
}
