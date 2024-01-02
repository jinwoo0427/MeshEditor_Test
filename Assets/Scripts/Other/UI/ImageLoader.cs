using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class ImageLoader : MonoBehaviour
{
    public string imagePath; // 이미지 파일의 경로
    public RawImage rawImage; // 이미지를 표시할 RawImage UI 요소

    void Start()
    {
        LoadImage();
    }

    void LoadImage()
    {
        if (File.Exists(imagePath))
        {
            // 이미지 파일이 존재하면 파일을 바이트 배열로 읽어옴
            byte[] imageData = File.ReadAllBytes(imagePath);

            // 읽어온 바이트 배열을 텍스처로 변환
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            // RawImage에 텍스처를 할당하여 이미지를 표시
            rawImage.texture = texture;
        }
        else
        {
            Debug.LogError("이미지 파일이 존재하지 않습니다. 경로를 확인해주세요.");
        }
    }
}