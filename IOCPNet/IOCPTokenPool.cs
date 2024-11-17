using System.Collections.Generic;

// IOCP会话连接的Token缓存池 IOCP Session Connection Token Cache Pool

namespace PENet
{
    public class IOCPTokenPool<T, K>
        where T : IOCPToken<K>, new()
        where K : IOCPMessage, new()
    {
        Stack<T> stack;
        public int size => stack.Count;

        public IOCPTokenPool(int capacity)
        {
            stack = new Stack<T>(capacity);
        }

        public T Pop()
        {
            lock (stack)
            {
                return stack.Pop();
            }
        }

        public void Push(T token)
        {
            if (token == null)
            {
                IOCPTool.ErrorLog("Token to push into the pool cannot be null.");
            }
            lock (stack)
            {
                stack.Push(token);
            }
        }
    }
}
