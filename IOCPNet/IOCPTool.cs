using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

/*
 * 核心类: 发起连接、接受连接
 * 连接池: 批量创建连接、回收连接
 * 连接: 数据收发管理
 * 工具: 日志显示、序列化与反序列化等
 */

// Tools for IOCP 工具
namespace PENet
{
    public static class IOCPTool
    {
        public static byte[] SplitLogicBytes(ref List<byte> bytesList)
        {
            byte[] buff = null;
            if (bytesList.Count > 4) // header is int(4 bytes)
            {
                // header: 存储后面数据长度的头部 header: store the length of data
                byte[] data = bytesList.ToArray();
                int length = BitConverter.ToInt32(data, 0); // Get the length of data.(获取数据长度 length)
                if (bytesList.Count >= length + 4)
                {
                    buff = new byte[length];
                    Buffer.BlockCopy(data, 4, buff, 0, length); // Start at 4 because its header is 4 bytes(int).
                    bytesList.RemoveRange(0, length + 4);
                }
            }
            return buff;
        }

        public static byte[] PackLengthInfo(byte[] body) // 给数据包加上头部信息(数据长度) Pack header information (data length) for data
        {
            int length = body.Length;
            byte[] packages = new byte[length + 4];
            byte[] header = BitConverter.GetBytes(length);
            header.CopyTo(packages, 0);
            body.CopyTo(packages, 4);
            return packages;
        }

        public static byte[] Serialize<T>(T msg) where T : IOCPMessage
        {
            byte[] data = null;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            try // 序列化msg 容易出错, 所以用try-catch
            {
                bf.Serialize(ms, msg);
                ms.Seek(0, SeekOrigin.Begin);
                data = ms.ToArray();
            }
            catch (SerializationException e)
            {
                ErrorLog("Serialization Failed! Reason:{0}", e.Message);
            }
            finally
            {
                ms.Close();
            }
            return data;
        }

        public static T Deserialize<T>(byte[] bytes) where T : IOCPMessage
        {
            T msg = null;
            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            try // 序列化msg 容易出错, 所以用try-catch
            {
                msg = (T)bf.Deserialize(ms);
            }
            catch (SerializationException e)
            {
                ErrorLog("Deserialization Failed! Reason:{0}, bytes length:{1}", e.Message, bytes.Length);
            }
            finally
            {
                ms.Close();
            }
            return msg;
        }

        /// <summary>
        /// 日志输出部分
        /// </summary>
        #region LOG
        public static Action<string> LogFunc;
        public static Action<IOCPLogColor, string> ColorLogFunc;
        public static Action<string> WarnFunc;
        public static Action<string> ErrorFunc;

        public static void Log(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (LogFunc != null)
            {
                LogFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPLogColor.None);
            }
        }
        public static void ColorLog(IOCPLogColor color, string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ColorLogFunc != null)
            {
                ColorLogFunc(color, msg);
            }
            else
            {
                ConsoleLog(msg, color);
            }
        }
        public static void WarnLog(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (WarnFunc != null)
            {
                WarnFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPLogColor.Yellow);
            }
        }
        public static void ErrorLog(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ErrorFunc != null)
            {
                ErrorFunc(msg);
            }
            else
            {
                ConsoleLog(msg, IOCPLogColor.Red);
            }
        }

        private static void ConsoleLog(string msg, IOCPLogColor color)
        {
            int threadID = Thread.CurrentThread.ManagedThreadId;
            msg = string.Format("Thread: {0} {1}", threadID, msg);
            switch (color)
            {
                case IOCPLogColor.Red:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Cyan:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Magenta:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.Yellow:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(msg);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case IOCPLogColor.None:
                default:
                    Console.WriteLine(msg);
                    break;
            }
        }
        #endregion
    }

    public enum IOCPLogColor
    {
        None,
        Red,
        Green,
        Blue,
        Cyan,
        Magenta,
        Yellow
    }
}
