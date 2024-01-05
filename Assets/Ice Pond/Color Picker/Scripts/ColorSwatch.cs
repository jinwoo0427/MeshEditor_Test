using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPickerUtil
{
    public class ColorSwatch : MonoBehaviour
    {
        [SerializeField] Transform buttonHolder;
        [SerializeField] RectTransform indicator;
        public Image targetGraphic;
        public Image sourceGraphic;
        public List<Color> colors = new List<Color>();
        public ColorPickerColorEvent onPickColor;

        ColorSwatchButton[] buttons;
        ColorSwatchButton selectedButton;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Deselect();
        }

        void Initialize()
        {
            buttons = buttonHolder.GetComponentsInChildren<ColorSwatchButton>(true);
            for (int i = 0; i < buttons.Length; ++i)
            {
                buttons[i].Initialize(this, i);
                if (i < colors.Count) buttons[i].SetColor(colors[i]);
                else buttons[i].gameObject.SetActive(false);
            }
            indicator.sizeDelta = buttonHolder.GetComponent<GridLayoutGroup>().cellSize;
        }

        void Refresh(int start = 0)
        {
            for (int i = start; i < buttons.Length; ++i)
            {
                if (i < colors.Count) buttons[i].SetColor(colors[i]);
                else buttons[i].gameObject.SetActive(false);
            }
        }

        public void Deselect()
        {
            selectedButton = null;
            indicator.gameObject.SetActive(false);
        }

        public void OnPickColor(ColorSwatchButton btn)
        {
            if (targetGraphic != null) targetGraphic.color = colors[btn.Index];
            selectedButton = btn;
            indicator.gameObject.SetActive(true);
            indicator.anchoredPosition = btn.GetComponent<RectTransform>().anchoredPosition;
            onPickColor.Invoke(colors[btn.Index]);
        }

        public void AddColor()
        {
            if (sourceGraphic == null) return;
            if (selectedButton == null)
            {
                if (colors.Count < buttons.Length)
                {
                    colors.Add(sourceGraphic.color);
                    int index = colors.Count - 1;
                    buttons[index].SetColor(sourceGraphic.color);
                    buttons[index].gameObject.SetActive(true);
                }
                else
                {
                    int index = colors.Count - 1;
                    colors[index] = sourceGraphic.color;
                    buttons[index].SetColor(sourceGraphic.color);
                }
            }
            else
            {
                colors[selectedButton.Index] = sourceGraphic.color;
                selectedButton.SetColor(sourceGraphic.color);
                Deselect();
            }
        }

        public void RemoveColor()
        {
            if (selectedButton == null)
            {
                if (colors.Count > 0)
                {
                    int index = colors.Count - 1;
                    colors.RemoveAt(colors.Count - 1);
                    buttons[index].gameObject.SetActive(false);
                }
            }
            else
            {
                colors.RemoveAt(selectedButton.Index);
                Refresh(selectedButton.Index);
                Deselect();
            }
        }
    }
}