using System;
using UnityEngine;

namespace XDPaint.Editor.Utils
{
    public class EditorInput
    {
        public Action<int> OnMouseDown;
        public Action<int> OnMouseDrag;
        public Action<int> OnMouseUp;
        public Action<int> OnRepaint;

        public void Update()
        {
            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var guiEvent = Event.current.GetTypeForControl(controlId);
            if (guiEvent == EventType.MouseDown)
            {
                OnMouseDown?.Invoke(controlId);
            }
            else if (guiEvent == EventType.MouseDrag)
            {
                OnMouseDrag?.Invoke(controlId);
            }
            else if (guiEvent == EventType.MouseUp || guiEvent == EventType.MouseLeaveWindow)
            {
                OnMouseUp?.Invoke(controlId);
            }
            else if (guiEvent == EventType.Repaint)
            {
                OnRepaint?.Invoke(controlId);
            }
        }
    }
}