using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class ImageLoader : MonoBehaviour
{
    public string imagePath; // �̹��� ������ ���
    public RawImage rawImage; // �̹����� ǥ���� RawImage UI ���

    void Start()
    {
        LoadImage();
    }

    void LoadImage()
    {
        if (File.Exists(imagePath))
        {
            // �̹��� ������ �����ϸ� ������ ����Ʈ �迭�� �о��
            byte[] imageData = File.ReadAllBytes(imagePath);

            // �о�� ����Ʈ �迭�� �ؽ�ó�� ��ȯ
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            // RawImage�� �ؽ�ó�� �Ҵ��Ͽ� �̹����� ǥ��
            rawImage.texture = texture;
        }
        else
        {
            Debug.LogError("�̹��� ������ �������� �ʽ��ϴ�. ��θ� Ȯ�����ּ���.");
        }
    }
}