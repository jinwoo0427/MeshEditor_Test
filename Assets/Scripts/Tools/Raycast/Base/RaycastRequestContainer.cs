using System.Collections.Generic;
using XDPaint.Core;
using XDPaint.Tools.Raycast.Data;

namespace XDPaint.Tools.Raycast.Base
{
    public class RaycastRequestContainer : IDisposable
    {
        public ulong RequestID;
        public IPaintManager Sender;
        public List<IRaycastRequest> RaycastRequests = new List<IRaycastRequest>();
        public int FingerId;
        public bool IsDisposed;

        private bool keyCached;
        private KeyValuePair<IPaintManager, int> key; 
        public KeyValuePair<IPaintManager, int> Key
        {
            get
            {
                if (!keyCached)
                {
                    key = new KeyValuePair<IPaintManager, int>(Sender, FingerId);
                    keyCached = true;
                }
                return key;
            }
        }

        public void DoDispose()
        {
            IsDisposed = true;
        }
    }
}