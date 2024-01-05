using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPickerUtil
{
    [RequireComponent(typeof(Button))]
    public class ColorSwatchButton : MonoBehaviour
    {
        ColorSwatch colorSwatch;
        Button btn;
        int index;
        public int Index { get { return index; } }

        public void Initialize(ColorSwatch colorSwatch, int index)
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
            this.colorSwatch = colorSwatch;
            this.index = index;
        }

        public void SetColor(Color c)
        {
            btn.image.color = c;
        }

        void OnClick()
        {
            colorSwatch.OnPickColor(this);
        }
    }
}