using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;

public class DragAreaIndicator : MonoBehaviour
{
    public RawImage dragIndicator; // 드래그 영역을 나타내는 이미지
    public RectTransform dragRectTransform;
    private RectTransform dragRect; // 드래그 중인 영역 저장

    private Vector2 dragStartPosition; // 드래그 시작 지점
    private Vector2 currentMousePosition; // 현재 마우스 위치

    public RectTransform dragArea; // 특정 영역을 나타내는 RectTransform
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

        // 마우스 포인터가 특정 영역 안에 있는지 확인
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

        // 드래그 크기가 음수인 경우 처리
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
        // 드래그한 영역의 좌표 및 크기를 획득
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
        // 텍스쳐에서 드래그한 영역을 복사하여 새로운 텍스쳐 생성
        Texture2D editedTexture = new Texture2D((int)_dragRect.width, (int)_dragRect.height);
        Color[] pixels = originalTexture.GetPixels((int)_dragRect.x, (int)_dragRect.y, (int)_dragRect.width, (int)_dragRect.height);
        editedTexture.SetPixels(pixels);
        editedTexture.Apply();

        // 새로운 텍스쳐를 대상 렌더러에 적용
        targetRenderer.texture = editedTexture;
    }
    Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        // 현재 활성 렌더 텍스처를 저장하고 새로운 RenderTexture로 설정
        // 새로운 Texture2D 생성 및 ReadPixels로 픽셀 복사
        Texture2D texture = new Texture2D(rt.width, rt.height);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // 원래의 활성 렌더 텍스처를 복원

        return texture;
    }
    public void EndDrag()
    {
        isDragging = false;
        EditTexture();
        //dragIndicator.gameObject.SetActive(false);
    }
}