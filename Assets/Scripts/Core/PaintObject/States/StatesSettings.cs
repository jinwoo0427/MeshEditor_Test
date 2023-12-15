using UnityEngine;
using XDPaint.Utils;

namespace XDPaint.States
{
    [CreateAssetMenu(fileName = "XDPaintStatesSettings", menuName = "XDPaint/States Settings", order = 103)]
    public class StatesSettings : SingletonScriptableObject<StatesSettings>
    {
        public bool UndoRedoEnabled = true;
        [Tooltip("Enable Undo/Redo for properties and actions. This might be helpful to save layers parameters changes like Opacity, Blending Mode, Merging layers etc.")] 
        public bool EnableUndoRedoForPropertiesAndActions = true;
        public int UndoRedoMaxActionsCount = 20;
    }
}