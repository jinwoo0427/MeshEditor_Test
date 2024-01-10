using RuntimeHandle;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Tools.Images.Base;
using static UnityEngine.GraphicsBuffer;

public class DragAreaIndicator : MonoBehaviour
{
    public RawImage dragIndicator; // 드래그 영역을 나타내는 이미지
    public RectTransform dragRectTransform;
    public RectTransform dragArea; // 특정 영역을 나타내는 RectTransform

    public GameObject DragHandleObj;
    public List<DragHandle> DragHandles ;

    private RectTransform dragRect; // 드래그 중인 영역 저장
    private Vector2 dragStartPosition; // 드래그 시작 지점
    private Vector2 currentMousePosition; // 현재 마우스 위치
    private Vector2 lastMousePosition;
    private bool isDragMoving = false;
    private bool isDragging = false;
    private bool isDragComplete = false;

    private Vector3 bottomLeft;
    private Vector3 topRight;
    private Texture2D originalTexture;
    
    
    void Start()
    {
        isDragMoving = false;
        isDragging = false;
        isDragComplete = false;
        dragRect = dragRectTransform;
        dragIndicator.gameObject.SetActive(false);
        DragHandleObj.gameObject.SetActive(false);

        Vector3[] corners = new Vector3[4];

        dragArea.GetLocalCorners(corners);

        bottomLeft = corners[0];
        topRight = corners[2];

        

    }

    void Update()
    {

        if (PaintController.Instance.GetCurPaintManager().Tool == XDPaint.Core.PaintTool.Selection )
        {
            if(isDragComplete == false)
                HandleDragInput();
            else
            {
                DragMoveInput();
                
            }
        }
        else if (PaintController.Instance.GetCurPaintManager().Tool != XDPaint.Core.PaintTool.Selection)
        {
            isDragging = false;
            isDragComplete = false;
            dragRect = dragRectTransform;
            dragIndicator.gameObject.SetActive(false);
            DragHandleObj.gameObject.SetActive(false);
        }

    }
    public void DragMoveInput()
    {
                
        foreach( var h in DragHandles )
        {
            if (h.IndragHanle)
            {
                isDragMoving = false;
                return;
            }
        }
        if (isDragMoving)
        {

            DragMoving();
        }
        if (IsMouseInDragArea(dragRect) && Input.GetMouseButtonDown(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out lastMousePosition);
            dragRect.pivot = new Vector2(0.5f, 0.5f);
            isDragMoving = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragMoving = false;

        }
    }
    public void HandleDragInput()
    {
        if (isDragging)
        {
            UpdateDrag();
        }
        else if (IsMouseInDragArea(dragArea) && Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }


    }
    void DragMoving()
    {

        Vector2 mousePos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out mousePos);

        Vector2 newAnchoredPosition = dragRect.anchoredPosition + mousePos - lastMousePosition;

        var wd = dragRect.rect.width / 2;
        var ht = dragRect.rect.height/ 2;

        newAnchoredPosition.x = Mathf.Clamp(newAnchoredPosition.x,-350f + wd, 350f - wd); 
        newAnchoredPosition.y = Mathf.Clamp(newAnchoredPosition.y, -350f + ht, 350f - ht); 

        dragRect.anchoredPosition = newAnchoredPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out lastMousePosition);
    }
    public bool IsMouseInDragArea(RectTransform rect)
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        // 마우스 포인터가 특정 영역 안에 있는지 확인
        return RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos);
    }
    public void StartDrag()
    {
        isDragging = true;
        dragIndicator.texture = null;
        dragIndicator.color = new Color(0f, 0f, 0f, 0.4f);
        dragStartPosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out dragStartPosition);
        dragRect.anchoredPosition = dragStartPosition;
        dragRect.sizeDelta = Vector2.zero;
        dragIndicator.gameObject.SetActive(true);

    }
    public void UpdateDrag()
    {
        currentMousePosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out currentMousePosition);

        currentMousePosition.x = Mathf.Clamp(currentMousePosition.x, bottomLeft.x, topRight.x);
        currentMousePosition.y = Mathf.Clamp(currentMousePosition.y, bottomLeft.y, topRight.y);

        Vector2 dragSize = currentMousePosition - dragStartPosition;
        dragRect.sizeDelta = new Vector2(Mathf.Abs(dragSize.x), Mathf.Abs(dragSize.y));
        // 드래그 크기가 음수인 경우 처리
        float newX = dragStartPosition.x + dragSize.x / 2;
        float newY = dragStartPosition.y + dragSize.y / 2;

        dragRect.anchoredPosition = new Vector2(newX, newY);

    }
    private void EditTexture()
    {
        // 드래그한 영역의 좌표 및 크기를 획득
        Rect _dragRect = GetDragRect(dragStartPosition, currentMousePosition, dragArea);

        var curPM = PaintController.Instance.GetCurPaintManager();
        RenderTexture tex  = curPM.LayersController.ActiveLayer.RenderTexture ;
        originalTexture = RenderTextureToTexture2D(tex);

        

        // 텍스쳐에서 드래그한 영역을 복사하여 새로운 텍스쳐 생성
        Texture2D editedTexture = new Texture2D((int)_dragRect.width, (int)_dragRect.height);
        Color[] pixels = originalTexture.GetPixels((int)_dragRect.x, (int)_dragRect.y, (int)_dragRect.width, (int)_dragRect.height);
        editedTexture.SetPixels(pixels);
        editedTexture.Apply();


        dragIndicator.color = Color.white;
        // 새로운 텍스쳐를 대상 렌더러에 적용
        dragIndicator.texture = editedTexture;


        // 드래그한 영역의 픽셀을 투명으로 채우기
        //Color[] transparentPixels = new Color[(int)_dragRect.width * (int)_dragRect.height];
        //for (int i = 0; i < transparentPixels.Length; i++)
        //{
        //    transparentPixels[i] = Color.clear; // 투명한 색상으로 채우기
        //}
        //originalTexture.SetPixels((int)_dragRect.x, (int)_dragRect.y, (int)_dragRect.width, (int)_dragRect.height, transparentPixels);
        //originalTexture.Apply();
        //Graphics.Blit(originalTexture, tex);


        var Data = PaintController.Instance.GetCurPaintManager().GetPaintData();
        Data.Material.SetTexture(Constants.PaintShader.PaintTexture, Data.LayersController.ActiveLayer.RenderTexture);
        Data.CommandBuilder.Clear().SetRenderTarget(Data.TexturesHelper.GetTarget(RenderTarget.ActiveLayerTemp)).ClearRenderTarget().
            DrawMesh(Data.QuadMesh, Data.Material, PaintPass.Paint, PaintPass.Erase).Execute();
        Data.Material.SetTexture(Constants.PaintShader.PaintTexture, Data.TexturesHelper.GetTexture(RenderTarget.ActiveLayerTemp));
    }
    private Rect GetDragRect(Vector2 dragStartPosition, Vector2 currentMousePosition, RectTransform canvasRectTransform)
    {
        Vector2 localStartPos, localEndPos;

        // 드래그 시작 지점과 현재 마우스 위치를 캔버스의 로컬 좌표로 변환
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, dragStartPosition, null, out localStartPos);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, currentMousePosition, null, out localEndPos);

        localStartPos = dragStartPosition;
        localEndPos =currentMousePosition;
        localStartPos += new Vector2(350f, 350f);
        localEndPos += new Vector2(350f, 350f);
        // 드래그한 영역의 Rect 계산
        float xMin = Mathf.Min(Mathf.RoundToInt( localStartPos.x), Mathf.RoundToInt(localEndPos.x));
        float yMin = Mathf.Min(Mathf.RoundToInt(localStartPos.y), Mathf.RoundToInt(localEndPos.y));
        float width = Mathf.Abs(localEndPos.x - localStartPos.x);
        float height = Mathf.Abs(localEndPos.y - localStartPos.y);

        return new Rect(xMin, yMin, width, height);
    }
    Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        // 현재 활성 렌더 텍스처를 저장하고 새로운 RenderTexture로 설정
        // 새로운 Texture2D 생성 및 ReadPixels로 픽셀 복사
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // 원래의 활성 렌더 텍스처를 복원
        RenderTexture.active = null;
        return texture;
    }
    public void EndDrag()
    {
        isDragging = false;
        isDragComplete = true;
        EditTexture();
        DragHandleObj.gameObject.SetActive(true);

        DragHandles.ForEach(x => x.InitHandle(dragRect, bottomLeft, topRight, dragStartPosition, dragArea));
        //dragIndicator.gameObject.SetActive(false);
    }
}