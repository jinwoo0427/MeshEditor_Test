using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPickerUtil
{
    public class Eyedropper : MonoBehaviour
    {
        [SerializeField] RectTransform blocker;
        public Image targetGraphic;
        public EyedropperPreview preview;
        [Tooltip("Keep/Discard preview image when sampling ends")]
        public bool keepPreview = true;
        public ColorPickerColorEvent onValueChanged;
        public ColorPickerColorEvent onEndSampling;

        [HideInInspector] public Color currentColor, newColor;
        Texture2D samplingTexture;
        int resolution;
        bool isSampling = false;

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            if (samplingTexture != null) Destroy(samplingTexture);
        }

        void Initialize()
        {
            if (preview == null)
            {
                samplingTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            }
            else
            {
                resolution = preview.resolution;
                int reso = resolution * 2 + 1;
                samplingTexture = new Texture2D(reso, reso, TextureFormat.RGB24, false);
                samplingTexture.filterMode = FilterMode.Point;
                preview.image.texture = samplingTexture;
            }
        }

        private void Update()
        {
            if (!isSampling) return;

            //cancel
            if (Input.GetKey(KeyCode.Escape)) { StopSampling(true); return; }

            //complete
            if (Input.GetMouseButton(0) || Input.touchCount == 1) { StopSampling(false); return; }

            //if (cursorImage != null) cursorImage.transform.position = Input.mousePosition;
        }

        public void StartSampling()
        {
            if (blocker == null) return;
            blocker.SetParent(transform.root, false);
            blocker.gameObject.SetActive(true);
            blocker.anchorMin = Vector2.zero;
            blocker.anchorMax = Vector2.one;
            blocker.anchoredPosition = Vector2.zero;
            blocker.sizeDelta = Vector2.zero;
            if (targetGraphic != null) currentColor = targetGraphic.color;
            if (preview != null) preview.image.texture = samplingTexture;
            isSampling = true;
            StartCoroutine(DoSampling());
        }

        public void StopSampling(bool isCanceled = false)
        {
            if (blocker == null) return;
            blocker.SetParent(transform, false);
            blocker.gameObject.SetActive(false);
            if (isCanceled) newColor = currentColor;
            if (targetGraphic != null) targetGraphic.color = newColor;
            if (preview != null && !keepPreview) preview.image.texture = null;
            onEndSampling.Invoke(newColor);
            isSampling = false;
        }

        IEnumerator DoSampling()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!isSampling) break;
                if (Input.mousePosition.x > resolution &&
                    Input.mousePosition.y > resolution &&
                    Input.mousePosition.x < Screen.width - resolution - 1 &&
                    Input.mousePosition.y < Screen.height - resolution - 1)
                {
                    Rect rect = new Rect(Input.mousePosition.x - resolution, Input.mousePosition.y - resolution, resolution * 2 + 1, resolution * 2 + 1);
                    samplingTexture.ReadPixels(rect, 0, 0, false);
                    samplingTexture.Apply();
                    newColor = samplingTexture.GetPixel(resolution, resolution);
                    onValueChanged.Invoke(newColor);
                    if (targetGraphic != null) targetGraphic.color = newColor;
                }
            }
        }
    }
}