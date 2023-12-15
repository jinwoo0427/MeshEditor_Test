namespace XDPaint.States
{
#if UNITY_EDITOR && XDP_DEBUG
    [System.Serializable]
#endif
    public class PropertyChangeRecord : BaseChangeRecord
    {
        private readonly object changedObject;
        private readonly string propertyName;
        private readonly object oldValue;
        private readonly object newValue;

        public PropertyChangeRecord(object obj, string propertyName, object oldValue, object newValue)
        {
            changedObject = obj;
            this.propertyName = propertyName;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public override void Undo()
        {
            var property = changedObject.GetType().GetProperty(propertyName);
            if (property.GetCustomAttributes(typeof(UndoRedoAttribute), true).Length == 0) 
                return;
            
            property.SetValue(changedObject, oldValue, null);
        }

        public override void Redo()
        {
            var property = changedObject.GetType().GetProperty(propertyName);
            if (property.GetCustomAttributes(typeof(UndoRedoAttribute), true).Length == 0) 
                return;

            property.SetValue(changedObject, newValue, null);
        }
    }
}