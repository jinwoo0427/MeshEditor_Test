using XDPaint.Core;

namespace XDPaint.Tools.Raycast.Data
{
    public interface IRaycastRequest : IDisposable
    {
        IPaintManager Sender { get; }
        int FingerId { get; }
        bool IsDisposed { get; }
    }
}