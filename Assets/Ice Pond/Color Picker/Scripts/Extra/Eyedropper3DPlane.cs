using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ColorPickerUtil
{
    public class Eyedropper3DPlane : MonoBehaviour
    {
        public GameObject plane;
        public Image targetGraphic;
        public EyedropperPreview preview;
        [Tooltip("Keep/Discard preview image when sampling ends")]
        public bool keepPreview = true;
        public ColorPickerColorEvent onValueChanged;
        public ColorPickerColorEvent onEndSampling;

        [HideInInspector] public Color currentColor, newColor;
        bool isSampling = false;
        int resolution;
        Texture tex;
        Texture2D samplingTexture;
        Texture2D t2D;

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
            if (plane == null) return;
            Renderer renderer = plane.GetComponent<Renderer>();
            if (renderer == null || renderer.material == null || renderer.material.mainTexture == null) return;
            tex = renderer.material.mainTexture;

            if (preview != null)
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
            if (tex == null) return;
            if (targetGraphic != null) currentColor = targetGraphic.color;
            if (preview != null) preview.image.texture = samplingTexture;
            t2D = GetTexture2D(tex);
            isSampling = true;
            StartCoroutine(DoSampling());
        }

        public void StopSampling(bool isCanceled = false)
        {
            if (tex == null) return;
            if (isCanceled) newColor = currentColor;
            if (targetGraphic != null) targetGraphic.color = newColor;
            if (preview != null && !keepPreview) preview.image.texture = null;
            onEndSampling.Invoke(newColor);
            isSampling = false; 
            DestroyImmediate(t2D);
        }

        IEnumerator DoSampling()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!isSampling) break;

                Ray touchRay = Camera.current.ScreenPointToRay(Input.mousePosition);
                RaycastHit raycasthit = new RaycastHit();
                bool hit = Physics.Raycast(touchRay, out raycasthit, 1000);
                if (hit && raycasthit.collider.gameObject == plane)
                {
                    int x = Mathf.RoundToInt(t2D.width * raycasthit.textureCoord.x);
                    int y = Mathf.RoundToInt(t2D.height * raycasthit.textureCoord.y);
                    newColor = t2D.GetPixel(x, y);

                    if (samplingTexture != null &&
                        x > resolution &&
                        y > resolution &&
                        x < t2D.width - resolution - 1 &&
                        y < t2D.height - resolution - 1)
                    {
                        Color[] colors = t2D.GetPixels(x - resolution, y - resolution, resolution * 2 + 1, resolution * 2 + 1);
                        samplingTexture.SetPixels(colors);
                        samplingTexture.Apply();
                    }
                }
                else
                {
                    newColor = Color.clear;
                }
                onValueChanged.Invoke(newColor);
                if (targetGraphic != null) targetGraphic.color = newColor;
            }
        }

        static Texture2D GetTexture2D(Texture tex)
        {
            Texture2D texture2D = new Texture2D(tex.width, tex.height);
            RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height, 32);
            Graphics.Blit(tex, rt);
            RenderTexture curr = RenderTexture.active;
            RenderTexture.active = rt;
            texture2D.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = curr;
            RenderTexture.ReleaseTemporary(rt);
            return texture2D;
        }
    }
}