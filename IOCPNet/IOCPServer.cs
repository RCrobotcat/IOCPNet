using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading;

// 基于IOCP封装的异步套接字通信的服务端实现 IOCP-based encapsulated asynchronous socket communication server implementation

namespace PENet
{
    public class IOCPServer
    {
        Socket skt;
        SocketAsyncEventArgs saea;

        int currentConnectionCount = 0; // 当前连接数
        Semaphore acceptSemaphore; // 信号量: 限制同时接受的连接数 Semaphore: Limit the number of connections accepted at the same time
        public int backlog = 100; // 最大挂起连接数(最大连接排队数)
        IOCPTokenPool tokenPool;
        List<IOCPToken> tokenList;

        public IOCPServer()
        {
            saea = new SocketAsyncEventArgs();
            saea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void StartAsyncServer(string ip, int port, int maxConnectionCount) // maxConnectionCount: 最大负载连接数
        {
            currentConnectionCount = 0;
            acceptSemaphore = new Semaphore(maxConnectionCount, maxConnectionCount);
            tokenPool = new IOCPTokenPool(maxConnectionCount);
            for (int i = 0; i < maxConnectionCount; i++)
            {
                IOCPToken token = new IOCPToken
                {
                    tokenID = i
                };
                tokenPool.Push(token);
            }
            tokenList = new List<IOCPToken>();

            IPEndPoint pt = new IPEndPoint(IPAddress.Parse(ip), port);
            skt = new Socket(pt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            skt.Bind(pt);
            skt.Listen(backlog);

            IOCPTool.ColorLog(IOCPLogColor.Green, "Server Start...");
            StartAccept();
        }

        void StartAccept()
        {
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
            IOCPToken token = tokenPool.Pop();
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

        /// <summary>
        /// 获取所有的连接
        /// </summary>
        public List<IOCPToken> GetTokenList()
        {
            return tokenList;
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessAccept();
        }
    }
}
