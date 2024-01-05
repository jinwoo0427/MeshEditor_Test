using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ColorPickerUtil
{
    public class ColorPickingButton : MonoBehaviour
    {
        public Image targetGraphic;
        public ColorPicker colorPicker;

        public ColorPickerColorEvent onEndPick;
        public UnityEvent onCancel;

        public void OnColorPickerOpen()
        {
            if (targetGraphic != null) colorPicker.currentColor = targetGraphic.color;
            colorPicker.onEndPick.AddListener(OnEndPick);
            colorPicker.onCancel.AddListener(OnCancel);
        }

        void OnEndPick(Color color)
        {
            if (targetGraphic != null) targetGraphic.color = color;
            onEndPick.Invoke(color);
            colorPicker.onEndPick.RemoveListener(OnEndPick);
            colorPicker.onCancel.RemoveListener(OnCancel);
        }

        void OnCancel()
        {
            onCancel.Invoke();
            colorPicker.onEndPick.RemoveListener(OnEndPick);
            colorPicker.onCancel.RemoveListener(OnCancel);
        }
    }
}