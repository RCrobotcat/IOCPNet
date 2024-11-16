using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        private Queue<byte[]> cacheQueue = new Queue<byte[]>(); // 缓存队列: 等到上一条消息处理完毕后再处理下一条消息, 先将下一条消息缓存起来
        // Cache queue: Wait until the previous message is processed before processing the next message, first cache the next message
        private bool isWriting = false; // 是否正在写入数据: 防止多线程同时写入数据, 造成数据错乱 
        // Whether writing data: Prevent multiple threads from writing data at the same time, causing data confusion

        private Socket skt;
        private SocketAsyncEventArgs rcvSaea;
        private SocketAsyncEventArgs sendSaea;

        public IOCPToken()
        {
            rcvSaea = new SocketAsyncEventArgs();
            sendSaea = new SocketAsyncEventArgs();
            rcvSaea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sendSaea.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
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

        public bool SendMsg(IOCPMessage msg)
        {
            byte[] bytes = IOCPTool.Serialize(msg);
            byte[] msgBytes = IOCPTool.PackLengthInfo(bytes);
            return SendMsg(msgBytes);
        }
        public bool SendMsg(byte[] bytes)
        {
            if (tokenState != TokenState.Connected)
            {
                IOCPTool.WarnLog("Connection is not available, counldn't send net message.");
                return false;
            }

            // 等到上一条消息处理完毕后再处理下一条消息, 先将下一条消息缓存起来
            if (isWriting)
            {
                cacheQueue.Enqueue(bytes);
                return true;
            }
            isWriting = true;

            sendSaea.SetBuffer(bytes, 0, bytes.Length);
            bool suspend = skt.SendAsync(sendSaea);
            if (!suspend)
            {
                ProcessSend();
            }
            return true;
        }
        void ProcessSend()
        {
            if (sendSaea.SocketError == SocketError.Success)
            {
                isWriting = false;
                if (cacheQueue.Count > 0)
                {
                    byte[] item = cacheQueue.Dequeue();
                    SendMsg(item);
                }
            }
            else
            {
                IOCPTool.ErrorLog("Process Send Error: {0}", sendSaea.SocketError.ToString());
                CloseToken();
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs saea)
        {
            switch (saea.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive();
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend();
                    break;
                default:
                    IOCPTool.WarnLog("The last operation completed on the socket was not a receive or send operation.");
                    break;
            }
        }

        public void CloseToken()
        {
            if (skt != null)
            {
                tokenState = TokenState.Disconnected;
                OnDisconnected();
                readList.Clear();
                cacheQueue.Clear();
                isWriting = false;

                try
                {
                    skt.Shutdown(SocketShutdown.Send);
                }
                catch (Exception e)
                {
                    IOCPTool.ErrorLog("Error on Shutdown Socket:{0}", e.ToString());
                }
                finally
                {
                    skt.Close();
                    skt = null;
                }
            }
        }

        void OnConnected()
        {
            IOCPTool.Log("Connection Success.");
        }

        void OnRecieveMessage(IOCPMessage msg)
        {
            IOCPTool.Log("Receive Message: {0}", msg.helloMessage);
        }

        void OnDisconnected()
        {
            IOCPTool.Log("Disconnected.");
        }
    }
}
