using System;

namespace XDPaint.States
{
    [Serializable]
    public class RenderTextureChangeReference
    {
        public readonly object ChangedObject;
        public readonly string PropertyName;
        public readonly int StateGroupIndex;
        
        public RenderTextureChangeReference(object entity, string property, int stateGroupIndex)
        {
            ChangedObject = entity;
            PropertyName = property;
            StateGroupIndex = stateGroupIndex;
        }
    }
}