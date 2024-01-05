using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class DragAreaIndicator : MonoBehaviour
{
    public Image dragIndicator; // 드래그 영역을 나타내는 이미지
    public RectTransform dragRectTransform;
    private RectTransform dragRect; // 드래그 중인 영역 저장

    private Vector2 dragStartPosition; // 드래그 시작 지점
    private Vector2 currentMousePosition; // 현재 마우스 위치

    public RectTransform dragArea; // 특정 영역을 나타내는 RectTransform
    public bool isDragging = false;

    private Vector3 bottomLeft;
    private Vector3 topRight;
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
        HandleDragInput();
    }

    void HandleDragInput()
    {
        if (isDragging)
        {
            UpdateDrag();
        }
        else if (IsMouseInDragArea() && Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }


    }
    bool IsMouseInDragArea()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        dragArea.GetWorldCorners(corners);

        // 마우스 포인터가 특정 영역 안에 있는지 확인
        return RectTransformUtility.RectangleContainsScreenPoint(dragArea, mousePos);
    }
    void StartDrag()
    {
        isDragging = true;
        dragStartPosition = Input.mousePosition;
        dragRect.anchoredPosition = dragStartPosition;
        dragRect.sizeDelta = Vector2.zero;
        dragIndicator.gameObject.SetActive(true);
    }

    void UpdateDrag()
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

    void EndDrag()
    {
        isDragging = false;
        //dragIndicator.gameObject.SetActive(false);
    }
}