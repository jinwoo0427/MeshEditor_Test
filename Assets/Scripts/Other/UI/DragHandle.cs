using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public RectTransform dragRect;
    public RectTransform handle;
    private RectTransform dragArea;
    public DragHandleType HandleType;
    private Vector3 bottomLeft;
    private Vector3 topRight;

    public bool isResizing;

    public bool IndragHanle = false;

    private Vector2 initialMousePosition;
    private void Start()
    {
        handle = GetComponent<RectTransform>();
        isResizing = false;
        IndragHanle = false;
    }
    public void InitHandle(RectTransform _dragRect, Vector3 bottom, Vector3 top , Vector2 dragStartPos , RectTransform area)
    {
        isResizing = false;
        IndragHanle = false;
        dragRect = _dragRect;
        bottomLeft = bottom;
        topRight = top;
        dragArea = area;
    }
    private void Update()
    {
        if (IsInMousePos())
        {
            IndragHanle = true;

            if (Input.GetMouseButtonDown(0))
            {
                isResizing = true;
                initialMousePosition = Input.mousePosition;
            }
        }
        else
        {
            IndragHanle = false;
        }

        if (isResizing)
        {
            if(IsInMousePos())
                ResizeHandle();

            if (Input.GetMouseButtonUp(0))
            {
                isResizing = false;
            }
        }
    }
    private bool IsInMousePos()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        handle.GetWorldCorners(corners);

        // 마우스 포인터가 특정 영역 안에 있는지 확인
        return RectTransformUtility.RectangleContainsScreenPoint(handle, mousePos);
    }
    private void ResizeHandle()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, initialMousePosition, null, out Vector2 localInitialMousePosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, currentMousePosition, null, out Vector2 localCurrentMousePosition);

        Vector2 offset = (localCurrentMousePosition - localInitialMousePosition) ;


        switch (HandleType)
        {
            case DragHandleType.Left:
                dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x - offset.x * 2f, dragRect.sizeDelta.y);
                dragRect.anchoredPosition += new Vector2(offset.x, 0f);
                break;
            case DragHandleType.Right:
                dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x + offset.x * 2f, dragRect.sizeDelta.y);
                dragRect.anchoredPosition += new Vector2(offset.x, 0f);
                break;
            case DragHandleType.Top:
                dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x, dragRect.sizeDelta.y + offset.y * 2f);
                dragRect.anchoredPosition += new Vector2(0f, offset.y);
                break;
            case DragHandleType.Bottom:
                dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x, dragRect.sizeDelta.y - offset.y * 2f);
                dragRect.anchoredPosition += new Vector2(0f, offset.y);
                break;
            case DragHandleType.LeftTop:
                ResizeLeftTopHandle(currentMousePosition);
                break;
            case DragHandleType.LeftBottom:
                ResizeLeftBottomHandle(currentMousePosition);
                break;
            case DragHandleType.RightTop:
                ResizeRightTopHandle(currentMousePosition);
                break;
            case DragHandleType.RightBottom:
                ResizeRightBottomHandle(currentMousePosition);
                break;
        }
        //initialMousePosition = Input.mousePosition;
    }

    private void ResizeLeftHandle(Vector2 currentMousePosition)
    {
        float newX = Mathf.Clamp(currentMousePosition.x, bottomLeft.x, topRight.x);
        float deltaWidth = dragRect.anchoredPosition.x - newX;
        dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x + deltaWidth, dragRect.sizeDelta.y);
        dragRect.anchoredPosition = new Vector2(newX, dragRect.anchoredPosition.y);
    }

    private void ResizeRightHandle(Vector2 currentMousePosition)
    {
        float newX = Mathf.Clamp(currentMousePosition.x, bottomLeft.x, topRight.x);
        float deltaWidth = newX - dragRect.anchoredPosition.x;
        dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x + deltaWidth, dragRect.sizeDelta.y);
    }

    private void ResizeTopHandle(Vector2 currentMousePosition)
    {
        float newY = Mathf.Clamp(currentMousePosition.y, bottomLeft.y, topRight.y);
        float deltaHeight = newY - dragRect.anchoredPosition.y;
        dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x, dragRect.sizeDelta.y + deltaHeight);
    }

    private void ResizeBottomHandle(Vector2 currentMousePosition)
    {
        float newY = Mathf.Clamp(currentMousePosition.y, bottomLeft.y, topRight.y);
        float deltaHeight = dragRect.anchoredPosition.y - newY;
        dragRect.sizeDelta = new Vector2(dragRect.sizeDelta.x, dragRect.sizeDelta.y + deltaHeight);
        dragRect.anchoredPosition = new Vector2(dragRect.anchoredPosition.x, newY);
    }

    private void ResizeLeftTopHandle(Vector2 currentMousePosition)
    {
        ResizeLeftHandle(currentMousePosition);
        ResizeTopHandle(currentMousePosition);
    }

    private void ResizeLeftBottomHandle(Vector2 currentMousePosition)
    {
        ResizeLeftHandle(currentMousePosition);
        ResizeBottomHandle(currentMousePosition);
    }

    private void ResizeRightTopHandle(Vector2 currentMousePosition)
    {
        ResizeRightHandle(currentMousePosition);
        ResizeTopHandle(currentMousePosition);
    }

    private void ResizeRightBottomHandle(Vector2 currentMousePosition)
    {
        ResizeRightHandle(currentMousePosition);
        ResizeBottomHandle(currentMousePosition);
    }
}
