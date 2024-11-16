using System.Collections.Generic;

// IOCP会话连接的Token缓存池 IOCP Session Connection Token Cache Pool

namespace PENet
{
    public class IOCPTokenPool
    {
        Stack<IOCPToken> stack;
        public int size => stack.Count;

        public IOCPTokenPool(int capacity)
        {
            stack = new Stack<IOCPToken>(capacity);
        }

        public IOCPToken Pop()
        {
            lock (stack)
            {
                return stack.Pop();
            }
        }

        public void Push(IOCPToken token)
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
