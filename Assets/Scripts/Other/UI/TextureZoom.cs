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

    [SerializeField] private DragAreaIndicator dragAreaIndicator;

    void Start()
    {
        if (rawImage == null)
        {
            Debug.LogError("집어넣기 까먹음");
            enabled = false;
            return;
        }

        rawImageRect = rawImage.GetComponent<RectTransform>();
        if (rawImageRect == null)
        {
            Debug.LogError("집어넣기 까먹음");
            enabled = false;
            return;
        }

        lastMousePos = Vector2.zero;
        isDragging = false;
    }

    void Update()
    {
        if (IsInMousePos(5f))
        {
            HandleDrag();
        }
    }
    public void InitPaintBoardUVRect()
    {
        Rect uvRect = new Rect(0f, 0f, 1f, 1f);
        rawImage.uvRect = uvRect;
    }

    void HandleDrag()
    {
        // 휠 클릭 상태 확인
        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        // 휠 클릭 상태일 때 이동
        if (isDragging && Input.GetMouseButton(2))
        {
            float dragX = -Input.GetAxis("Mouse X") * dragSpeed;
            float dragY = -Input.GetAxis("Mouse Y") * dragSpeed;

            // 현재 uvRect 가져오기
            Rect uvRect = rawImage.uvRect;

            // 새로운 uvRect의 위치 계산
            uvRect.x = Mathf.Clamp(uvRect.x + dragX, 0.0f, 1.0f - uvRect.width);
            uvRect.y = Mathf.Clamp(uvRect.y + dragY, 0.0f, 1.0f - uvRect.height);

            // 새로운 uvRect 적용
            rawImage.uvRect = uvRect;
        }
    }
    private bool IsInMousePos(float padding)
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        rawImageRect.GetWorldCorners(corners);
        Rect rect = new Rect(corners[0].x - padding, corners[0].y - padding,
                             corners[2].x - corners[0].x + 2 * padding,
                             corners[2].y - corners[0].y + 2 * padding);

        return rect.Contains(mousePos);
    }
    public void OnScroll(PointerEventData eventData)
    {
        if(dragAreaIndicator.isEnable)
        {
            dragAreaIndicator.AddEditTexture();
            dragAreaIndicator.Init();
        }    
        float scrollDelta = eventData.scrollDelta.y;

        // 마우스 위치 차이 계산
        Vector2 mousePos = Input.mousePosition;
        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRect, mousePos, null, out localMousePos))
        {
            // 현재 uvRect 가져오기
            Rect uvRect = rawImage.uvRect;

            // 새로운 uvRect의 크기 계산
            float targetWidth = Mathf.Clamp(uvRect.width - scrollDelta * zoomSpeed * uvRect.width, 0.1f, 1.0f);
            float targetHeight = Mathf.Clamp(uvRect.height - scrollDelta * zoomSpeed * uvRect.height, 0.1f, 1.0f);


            // 새로운 uvRect의 중심을 계산
            float targetX = Mathf.Clamp(uvRect.x + localMousePos.x * uvRect.width * scrollDelta * zoomSpeed, 0.0f, 1.0f - targetWidth);
            float targetY = Mathf.Clamp(uvRect.y + localMousePos.y * uvRect.height * scrollDelta * zoomSpeed, 0.0f, 1.0f - targetHeight);

            // 부드럽게 전환
            uvRect.width = Mathf.Lerp(uvRect.width, targetWidth, Time.deltaTime * smoothSpeed);
            uvRect.height = Mathf.Lerp(uvRect.height, targetHeight, Time.deltaTime * smoothSpeed);
            uvRect.x = Mathf.Lerp(uvRect.x, targetX, Time.deltaTime * smoothSpeed);
            uvRect.y = Mathf.Lerp(uvRect.y, targetY, Time.deltaTime * smoothSpeed);

            // 새로운 uvRect 적용
            rawImage.uvRect = uvRect;

            // 마우스 위치 업데이트
            lastMousePos = mousePos;
        }
    }
}
