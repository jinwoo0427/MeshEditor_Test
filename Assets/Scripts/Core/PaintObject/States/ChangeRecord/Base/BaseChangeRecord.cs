namespace XDPaint.States
{
    public abstract class BaseChangeRecord
    {
        public abstract void Undo();
        public abstract void Redo();
    }
}