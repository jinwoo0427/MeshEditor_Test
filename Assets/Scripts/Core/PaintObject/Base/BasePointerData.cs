namespace XDPaint.Core.PaintObject.Base
{
    public class BasePointerData
    {
        public int FingerId;

        protected BasePointerData(int fingerId)
        {
            FingerId = fingerId;
        }
    }
}