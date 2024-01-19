using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using Cursor = UnityEngine.Cursor;

public enum DragHandleType
{
    Left,
    Right,
    Top,
    Bottom,
    LeftTop,
    LeftBottom,
    RightTop,
    RightBottom,
}

public class DragHandle : MonoBehaviour
{
    private RectTransform dragRect;
    private Vector3 bottomLeft;
    private Vector3 topRight;
    private RectTransform handle;
    private RectTransform dragArea;
    private Vector2 initialMousePosition;
    private Vector2 initialDragRectPosition;
    private Vector2 initialDragRectSize;
    private bool isResizing = false;

    public DragHandleType HandleType;
    public bool IndragHandle = false;
    public bool isLimit = false;

    // 마우스 움직임에 대한 민감도 조절을 위한 계수
    public float sensitivity = 0.5f;

    public Texture2D customCursor;


    private void Start()
    {
        isResizing = false; 
        IndragHandle = false; 
        isLimit = false;
        handle = GetComponent<RectTransform>();

       
    }
    public void InitHandle(RectTransform _dragRect, Vector3 bottom, Vector3 top, Vector2 dragStartPos, RectTransform area)
    {
        isLimit = false;
        isResizing = false; 
        IndragHandle = false; 
        dragRect = _dragRect; 
        bottomLeft = bottom; 
        topRight = top;
        dragArea = area; 
    }
    private void Update()
    {
        if (IsInMousePos(5f, handle))
        {
            IndragHandle = true;
            if (Input.GetMouseButtonDown(0))
            {
                initialMousePosition = Input.mousePosition;
                initialDragRectPosition = dragRect.anchoredPosition;
                initialDragRectSize = dragRect.sizeDelta;
                isResizing = true;
            }
        }
        else
        {
            IndragHandle = false;
        }

        if (isResizing)
        {
            if(IsInMousePos(5f, dragArea))
                ResizeHandle();
            
            if (Input.GetMouseButtonUp(0))
            {
                isResizing = false;
            }
        }
    }

    private bool IsInMousePos(float padding , RectTransform rectTransform)
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        Rect rect = new Rect(corners[0].x - padding, corners[0].y - padding,
                             corners[2].x - corners[0].x + 2 * padding,
                             corners[2].y - corners[0].y + 2 * padding);

        return rect.Contains(mousePos);
    }

    private void ResizeHandle()
    {
        Vector2 currentMousePosition = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, initialMousePosition, null, out Vector2 localInitialMousePosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, currentMousePosition, null, out Vector2 localCurrentMousePosition);


        Vector2 offset = (localCurrentMousePosition - localInitialMousePosition) * sensitivity;

        Vector2 tempSizeDelta = dragRect.sizeDelta;
        Vector2 tempAnchoredPosition = dragRect.anchoredPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragArea, handle.position, null, out Vector2 handlePos);

        switch (HandleType)
        {
            case DragHandleType.Left:
                    LeftMove(offset);
                break;
            case DragHandleType.Right:
                    RightMove(offset);
                break;
            case DragHandleType.Top:
                    TopMove(offset);
                break;
            case DragHandleType.Bottom:
                    BottomMove(offset);
                break;
            case DragHandleType.LeftTop:
                dragRect.sizeDelta = new Vector2(initialDragRectSize.x - offset.x * 2f, initialDragRectSize.y + offset.y * 2f);
                dragRect.anchoredPosition = initialDragRectPosition + new Vector2(offset.x, offset.y);
                break;
            case DragHandleType.LeftBottom:
                dragRect.sizeDelta = new Vector2(initialDragRectSize.x - offset.x * 2f, initialDragRectSize.y - offset.y * 2f);
                dragRect.anchoredPosition = initialDragRectPosition + new Vector2(offset.x, offset.y);
                break;
            case DragHandleType.RightTop:
                dragRect.sizeDelta = new Vector2(initialDragRectSize.x + offset.x * 2f, initialDragRectSize.y + offset.y * 2f);
                dragRect.anchoredPosition = initialDragRectPosition + new Vector2(offset.x, offset.y);
                break;
            case DragHandleType.RightBottom:
                dragRect.sizeDelta = new Vector2(initialDragRectSize.x + offset.x * 2f, initialDragRectSize.y - offset.y * 2f);
                dragRect.anchoredPosition = initialDragRectPosition + new Vector2(offset.x, offset.y);
                break;
        }
        Vector2 newDragRectSize = dragRect.sizeDelta;

        newDragRectSize.x = Mathf.Clamp(newDragRectSize.x, 0f, 700f - dragRect.anchoredPosition.x);
        newDragRectSize.y = Mathf.Clamp(newDragRectSize.y, 0f, 700f - -dragRect.anchoredPosition.y);

        dragRect.sizeDelta = newDragRectSize;

        Vector2 newAnchoredPosition = dragRect.anchoredPosition;
        var wd = dragRect.rect.width / 2;
        var ht = dragRect.rect.height / 2;

        newAnchoredPosition.x = Mathf.Clamp(newAnchoredPosition.x, -350f + wd, 350f - wd);
        newAnchoredPosition.y = Mathf.Clamp(newAnchoredPosition.y, -350f + ht, 350f - ht);

        dragRect.anchoredPosition = newAnchoredPosition;

        //if (Mathf.Abs(dragRect.anchoredPosition.x) + wd >= 350f ||
        //    Mathf.Abs(dragRect.anchoredPosition.y) + ht >= 350f)
        //{
        //    dragRect.anchoredPosition = tempAnchoredPosition;
        //    dragRect.sizeDelta = tempSizeDelta;
        //}

    }
   
    private void BottomMove(Vector2 offset)
    {
        dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x, initialDragRectSize.y - offset.y * 2f);
        dragRect.anchoredPosition = initialDragRectPosition + new Vector2(0f, offset.y);
    }

    private void TopMove(Vector2 offset)
    {
        dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x, initialDragRectSize.y + offset.y * 2f);
        dragRect.anchoredPosition = initialDragRectPosition + new Vector2(0f, offset.y);
    }

    private void RightMove(Vector2 offset)
    {
        dragRect.sizeDelta = new Vector2(initialDragRectSize.x + offset.x * 2f, dragRect.sizeDelta.y);
        dragRect.anchoredPosition = initialDragRectPosition + new Vector2(offset.x, 0f);
    }

    private void LeftMove(Vector2 offset)
    {
        dragRect.sizeDelta = new Vector2(initialDragRectSize.x - offset.x * 2f, dragRect.sizeDelta.y);
        dragRect.anchoredPosition = initialDragRectPosition + new Vector2(offset.x, 0f);
    }

    public void CheckLimit()
    {
        
    }
}