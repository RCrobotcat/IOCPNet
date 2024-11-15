using PENet;
using System.Net.Sockets;
using System;

// IOCP连接会话的Token IOCP Connection Session Token
// 用于处理连接会话的数据传输和处理 Used for data transmission and processing of connection sessions

namespace IOCPNet
{
    public enum TokenState
    {
        None,
        Connected,
        Disconnected,
    }

    public class IOCPToken
    {
        public int tokenID;
        public TokenState tokenState = TokenState.None;

        private Socket skt;
        private SocketAsyncEventArgs rcvSaea;

        public IOCPToken()
        {
            rcvSaea = new SocketAsyncEventArgs();
            rcvSaea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void InitToken(Socket skt)
        {
            this.skt = skt;
            tokenState = TokenState.Connected;
            OnConnected();

            StartAsyncReceive();
        }

        void StartAsyncReceive()
        {
            bool suspend = skt.ReceiveAsync(rcvSaea);
            if (!suspend)
            {
                ProcessReceive();
            }
        }

        void ProcessReceive()
        {
            // TODO
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessReceive();
        }

        void OnConnected()
        {
            IOCPTool.Log("Connection Success.");
        }
    }
}
