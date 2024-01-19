using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using UnityEngine.Networking;
using static UnityEditor.Rendering.CameraUI;
using GetampedPaint;
using GetampedPaint.Controllers;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using GetampedPaint.Core.Layers;

[RequireComponent(typeof(Button))]
public class CanvasSampleOpenFileImage : MonoBehaviour, IPointerDownHandler {
    public RawImage output;
    public GameObject DrawPanel;

#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".png, .jpg", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
    //
    // Standalone platforms & editor
    //
    public void OnPointerDown(PointerEventData eventData) { }

    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var paths = StandaloneFileBrowser.OpenFilePanel("OpenImage", "", "", false);
        if (paths.Length > 0) {
            StartCoroutine(OutputRoutine(new System.Uri(paths[0]).AbsoluteUri));
        }
    }
#endif

    private IEnumerator OutputRoutine(string url) {
        //var loader = new WWW(url);
        //yield return loader;
        //output.texture = loader.texture;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                var layersController = (LayersController)PaintController.Instance.GetCurPaintManager().LayersController;

                // 레이어 생성 제한 갯수가 넘어갈때
                if (layersController.Layers.Count >= layersController.MaxLayersCount)
                {
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                     layersController.AddLayerImage(texture, rect);
                }
                else
                {
                    if (texture.width > 512 || texture.height > 512)
                    {
                        //layersController.AddNewLayer("ImportImage", texture, true);
                    }
                    layersController.AddNewLayer("ImportImage");
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    layersController.AddLayerImage(texture, rect);
                }


                output.texture = texture;
            }
            else
            {
                Debug.LogError("Failed to load image: " + www.error);
            }
        }

        //Input.ResetInputAxes();
    }

}
