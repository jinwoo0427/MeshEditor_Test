using System;

namespace XDPaint.States
{
#if UNITY_EDITOR && XDP_DEBUG
    [Serializable]
#endif
    public class ActionRecord : BaseChangeRecord
    {
        private Action action;

        public ActionRecord(Action action)
        {
            this.action = action;
        }

        public override void Redo()
        {
            action?.Invoke();
        }

        public override void Undo()
        {
            
        }
    }
}