using System.Net.Sockets;
using System;
using System.Collections.Generic;

// IOCP连接会话的Token IOCP Connection Session Token
// 用于处理连接会话的数据传输和处理 Used for data transmission and processing of connection sessions

namespace PENet
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
        private List<byte> readList = new List<byte>();
        public TokenState tokenState = TokenState.None;

        private Socket skt;
        private SocketAsyncEventArgs rcvSaea;

        public IOCPToken()
        {
            rcvSaea = new SocketAsyncEventArgs();
            rcvSaea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            rcvSaea.SetBuffer(new byte[2048], 0, 2048); // Set the buffer for data receiving. 设置接收数据的缓冲区
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
            if (rcvSaea.BytesTransferred > 0 && rcvSaea.SocketError == SocketError.Success)
            {
                byte[] bytes = new byte[rcvSaea.BytesTransferred];
                Buffer.BlockCopy(rcvSaea.Buffer, 0, bytes, 0, rcvSaea.BytesTransferred);
                readList.AddRange(bytes);
                ProcessByteList();
                StartAsyncReceive(); // Continue to receive data.
            }
            else
            {
                IOCPTool.WarnLog("Token: {0} Close:{1}", tokenID, rcvSaea.SocketError.ToString());
                CloseToken();
            }
        }

        void ProcessByteList()
        {
            byte[] buff = IOCPTool.SplitLogicBytes(ref readList);
            if (buff != null)
            {
                IOCPMessage msg = IOCPTool.Deserialize(buff);
                OnRecieveMessage(msg);
                ProcessByteList(); // Continue to process the remaining data.
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            ProcessReceive();
        }

        public void CloseToken()
        {
            // TODO
        }

        void OnConnected()
        {
            IOCPTool.Log("Connection Success.");
        }

        void OnRecieveMessage(IOCPMessage msg)
        {
            IOCPTool.Log("Receive Message: {0}", msg.helloMessage);
        }
    }
}
