using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPickerUtil
{
    public class EyedropperPreview : MonoBehaviour
    {
        public RawImage image;
        public Image grid;
        public Image gridCenter;
        [Range(1, 10)] public float gridLineWidth = 2.0f;
        [Range(1, 10)] public int resolution = 5;

        private void Awake()
        {
            RefreshGrid();
        }

        private void OnEnable()
        {
            RefreshGrid();
        }

        private void OnValidate()
        {
            RefreshGrid();
        }

        public void RefreshGrid()
        {
            int reso = resolution * 2 + 1;
            gridCenter.rectTransform.sizeDelta = new Vector2(image.rectTransform.rect.width / reso, image.rectTransform.rect.height / reso);
            grid.material.SetFloat("_NumCell", reso);
            grid.material.SetFloat("_LineWidth", gridLineWidth / 10.0f);
        }
    }
}