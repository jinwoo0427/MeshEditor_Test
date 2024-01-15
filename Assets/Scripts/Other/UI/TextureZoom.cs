using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextureZoom : MonoBehaviour, IScrollHandler
{
    public RawImage rawImage;
    public float zoomSpeed = 0.1f;
    public float smoothSpeed = 5f;
    public float dragSpeed = 0.1f;

    private RectTransform rawImageRect;
    private Vector2 lastMousePos;
    private bool isDragging;

    void Start()
    {
        if (rawImage == null)
        {
            Debug.LogError("RawImage is not assigned to the script or is missing.");
            enabled = false;
            return;
        }

        rawImageRect = rawImage.GetComponent<RectTransform>();
        if (rawImageRect == null)
        {
            Debug.LogError("RawImage does not have a RectTransform component.");
            enabled = false;
            return;
        }

        lastMousePos = Vector2.zero;
        isDragging = false;
    }

    void Update()
    {
        HandleDrag();
    }

    void HandleDrag()
    {
        // �� Ŭ�� ���� Ȯ��
        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        // �� Ŭ�� ������ �� �̵�
        if (isDragging && Input.GetMouseButton(2))
        {
            float dragX = -Input.GetAxis("Mouse X") * dragSpeed;
            float dragY = -Input.GetAxis("Mouse Y") * dragSpeed;

            // ���� uvRect ��������
            Rect uvRect = rawImage.uvRect;

            // ���ο� uvRect�� ��ġ ���
            uvRect.x = Mathf.Clamp(uvRect.x + dragX, 0.0f, 1.0f - uvRect.width);
            uvRect.y = Mathf.Clamp(uvRect.y + dragY, 0.0f, 1.0f - uvRect.height);

            // ���ο� uvRect ����
            rawImage.uvRect = uvRect;
        }
    }
    public void OnScroll(PointerEventData eventData)
    {
        float scrollDelta = eventData.scrollDelta.y;

        // ���콺 ��ġ ���� ���
        Vector2 mousePos = Input.mousePosition;
        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, mousePos, null, out localMousePos))
        {
            // ���� uvRect ��������
            Rect uvRect = rawImage.uvRect;

            // ���ο� uvRect�� ũ�� ���
            float targetWidth = Mathf.Clamp(uvRect.width - scrollDelta * zoomSpeed * uvRect.width, 0.1f, 1.0f);
            float targetHeight = Mathf.Clamp(uvRect.height - scrollDelta * zoomSpeed * uvRect.height, 0.1f, 1.0f);

            // ���ο� uvRect�� �߽��� ���
            float targetX = Mathf.Clamp(uvRect.x + localMousePos.x * uvRect.width * scrollDelta * zoomSpeed, 0.0f, 1.0f - targetWidth);
            float targetY = Mathf.Clamp(uvRect.y + localMousePos.y * uvRect.height * scrollDelta * zoomSpeed, 0.0f, 1.0f - targetHeight);

            // �ε巴�� ��ȯ
            uvRect.width = Mathf.Lerp(uvRect.width, targetWidth, Time.deltaTime * smoothSpeed);
            uvRect.height = Mathf.Lerp(uvRect.height, targetHeight, Time.deltaTime * smoothSpeed);
            uvRect.x = Mathf.Lerp(uvRect.x, targetX, Time.deltaTime * smoothSpeed);
            uvRect.y = Mathf.Lerp(uvRect.y, targetY, Time.deltaTime * smoothSpeed);

            // ���ο� uvRect ����
            rawImage.uvRect = uvRect;

            // ���콺 ��ġ ������Ʈ
            lastMousePos = mousePos;
        }
    }
}
