using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class DragAreaIndicator : MonoBehaviour
{
    public Image dragIndicator; // �巡�� ������ ��Ÿ���� �̹���
    public RectTransform dragRectTransform;
    private RectTransform dragRect; // �巡�� ���� ���� ����

    private Vector2 dragStartPosition; // �巡�� ���� ����
    private Vector2 currentMousePosition; // ���� ���콺 ��ġ

    public RectTransform dragArea; // Ư�� ������ ��Ÿ���� RectTransform
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

        // ���콺 �����Ͱ� Ư�� ���� �ȿ� �ִ��� Ȯ��
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

    void EndDrag()
    {
        isDragging = false;
        //dragIndicator.gameObject.SetActive(false);
    }
}