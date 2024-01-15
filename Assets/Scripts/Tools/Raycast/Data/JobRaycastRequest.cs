using Unity.Collections;
using Unity.Jobs;
using GetampedPaint.Core;

namespace GetampedPaint.Tools.Raycast.Data
{
    public class JobRaycastRequest : IRaycastRequest
    {
        public ulong RequestId;
        public JobHandle JobHandle;
        public NativeArray<TriangleData> InputNativeArray;
        public NativeArray<RaycastTriangleData> OutputNativeArray;

        public IPaintManager Sender { get; set; }
        public int FingerId { get; set; }
        public bool IsDisposed { get; private set; }
        
        public void DoDispose()
        {
            if (!JobHandle.IsCompleted)
            {
                JobHandle.Complete();
            }
            
            if (InputNativeArray.IsCreated)
            {
                try
                {
                    InputNativeArray.Dispose();
                }
                catch
                {
                    // ignored
                }
            }
            
            if (OutputNativeArray.IsCreated)
            {
                try
                {
                    OutputNativeArray.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            IsDisposed = true;
        }
    }
}