using System;
using UnityEditor;
using UnityEngine;

namespace XDPaint.Editor.Utils
{
    public class EnumDrawer<T> where T : struct
    {
        private string[] modes;
        private int modeId;
        private GUIContent[] modesContent;
        private T mode;

        public int ModeId => modeId;

        public void Init()
        {
            modes = Enum.GetNames(typeof(T));
            modesContent = new GUIContent[modes.Length];
            for (var i = 0; i < modesContent.Length; i++)
            {
                modesContent[i] = new GUIContent(modes[i]);
            }
        }

        public bool Draw(SerializedProperty property, string label, string tip, ref T result)
        {
            
            EditorGUI.BeginChangeCheck();
            modeId = EditorGUILayout.Popup(new GUIContent(label, tip), property.enumValueIndex, modesContent);
            if (EditorGUI.EndChangeCheck())
            {
                mode = (T)Enum.Parse(typeof(T), modes[modeId]);
                result = mode;
                return true;
            }
            return false;
        }
    }
}