using System.Collections.Generic;
using GetampedPaint.Core;

namespace GetampedPaint.Tools.Raycast.Data
{
    public class CPURaycastRequest : IRaycastRequest
    {
        public ulong RequestId;
        public List<RaycastTriangleData> OutputList;

        public IPaintManager Sender { get; set; }
        public int FingerId { get; set; }
        public bool IsDisposed { get; private set; }
        
        public void DoDispose()
        {
            OutputList.Clear();
            OutputList = null;
            IsDisposed = true;
        }
    }
}