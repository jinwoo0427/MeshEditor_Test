using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;

public class DragAreaIndicator : MonoBehaviour
{
    public RawImage dragIndicator; // �巡�� ������ ��Ÿ���� �̹���
    public RectTransform dragRectTransform;
    private RectTransform dragRect; // �巡�� ���� ���� ����

    private Vector2 dragStartPosition; // �巡�� ���� ����
    private Vector2 currentMousePosition; // ���� ���콺 ��ġ

    public RectTransform dragArea; // Ư�� ������ ��Ÿ���� RectTransform
    public bool isDragging = false;

    private Vector3 bottomLeft;
    private Vector3 topRight;
    public Texture2D originalTexture;
    public RawImage targetRenderer;

    void Start()
    {
        isDragging = false;
        dragRect = dragRectTransform;
        dragIndicator.gameObject.SetActive(false);

        Vector3[] corners = new Vector3[4];

        dragArea.GetWorldCorners(corners);

        bottomLeft = corners[0];
        topRight = corners[2];

        

    }

    void Update()
    {
        if(PaintController.Instance.GetCurPaintManager().Tool == XDPaint.Core.PaintTool.Selection)
            HandleDragInput();
    }

    public void HandleDragInput()
    {
        if (isDragging)
        {
            UpdateDrag();
        }
        else if (IsMouseInDragArea() && Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }


    }
    public bool IsMouseInDragArea()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        dragArea.GetWorldCorners(corners);

        // ���콺 �����Ͱ� Ư�� ���� �ȿ� �ִ��� Ȯ��
        return RectTransformUtility.RectangleContainsScreenPoint(dragArea, mousePos);
    }
    public void StartDrag()
    {
        isDragging = true;
        dragStartPosition = Input.mousePosition;
        dragRect.anchoredPosition = dragStartPosition;
        dragRect.sizeDelta = Vector2.zero;
        dragIndicator.gameObject.SetActive(true);

    }
    public void UpdateDrag()
    {
        currentMousePosition = Input.mousePosition;

        currentMousePosition.x = Mathf.Clamp(currentMousePosition.x, bottomLeft.x, topRight.x);
        currentMousePosition.y = Mathf.Clamp(currentMousePosition.y, bottomLeft.y, topRight.y);

        Vector2 dragSize = currentMousePosition - dragStartPosition;
        dragRect.sizeDelta = new Vector2(Mathf.Abs(dragSize.x), Mathf.Abs(dragSize.y));

        // �巡�� ũ�Ⱑ ������ ��� ó��
        if (dragSize.x < 0)
        {
            dragRect.pivot = new Vector2(1, dragRect.pivot.y);
        }
        else
        {
            dragRect.pivot = new Vector2(0, dragRect.pivot.y);
        }

        if (dragSize.y < 0)
        {
            dragRect.pivot = new Vector2(dragRect.pivot.x, 1);
        }
        else
        {
            dragRect.pivot = new Vector2(dragRect.pivot.x, 0);
        }

    }
    private void EditTexture()
    {
        // �巡���� ������ ��ǥ �� ũ�⸦ ȹ��
        Rect _dragRect = new Rect(
            dragRect.anchoredPosition.x,
            dragRect.anchoredPosition.y,
            dragRect.sizeDelta.x,
            dragRect.sizeDelta.y
        );
        Debug.Log(_dragRect);
        RenderTexture tex  = PaintController.Instance.GetCurPaintManager().LayersController.ActiveLayer.RenderTexture ;
        originalTexture = RenderTextureToTexture2D(tex);
        Debug.Log(originalTexture.width + " : " +  originalTexture.height);
        // �ؽ��Ŀ��� �巡���� ������ �����Ͽ� ���ο� �ؽ��� ����
        Texture2D editedTexture = new Texture2D((int)_dragRect.width, (int)_dragRect.height);
        Color[] pixels = originalTexture.GetPixels((int)_dragRect.x, (int)_dragRect.y, (int)_dragRect.width, (int)_dragRect.height);
        editedTexture.SetPixels(pixels);
        editedTexture.Apply();

        // ���ο� �ؽ��ĸ� ��� �������� ����
        targetRenderer.texture = editedTexture;
    }
    Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        // ���� Ȱ�� ���� �ؽ�ó�� �����ϰ� ���ο� RenderTexture�� ����
        // ���ο� Texture2D ���� �� ReadPixels�� �ȼ� ����
        Texture2D texture = new Texture2D(rt.width, rt.height);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // ������ Ȱ�� ���� �ؽ�ó�� ����

        return texture;
    }
    public void EndDrag()
    {
        isDragging = false;
        EditTexture();
        //dragIndicator.gameObject.SetActive(false);
    }
}