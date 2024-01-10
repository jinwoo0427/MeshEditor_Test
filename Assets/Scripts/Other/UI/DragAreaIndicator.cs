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
    public RawImage dragIndicator; // �巡�� ������ ��Ÿ���� �̹���
    public RectTransform dragRectTransform;
    public RectTransform dragArea; // Ư�� ������ ��Ÿ���� RectTransform

    public GameObject DragHandleObj;
    public List<DragHandle> DragHandles ;

    private RectTransform dragRect; // �巡�� ���� ���� ����
    private Vector2 dragStartPosition; // �巡�� ���� ����
    private Vector2 currentMousePosition; // ���� ���콺 ��ġ
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

        // ���콺 �����Ͱ� Ư�� ���� �ȿ� �ִ��� Ȯ��
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
        // �巡�� ũ�Ⱑ ������ ��� ó��
        float newX = dragStartPosition.x + dragSize.x / 2;
        float newY = dragStartPosition.y + dragSize.y / 2;

        dragRect.anchoredPosition = new Vector2(newX, newY);

    }
    private void EditTexture()
    {
        // �巡���� ������ ��ǥ �� ũ�⸦ ȹ��
        Rect _dragRect = GetDragRect(dragStartPosition, currentMousePosition, dragArea);

        var curPM = PaintController.Instance.GetCurPaintManager();
        RenderTexture tex  = curPM.LayersController.ActiveLayer.RenderTexture ;
        originalTexture = RenderTextureToTexture2D(tex);

        

        // �ؽ��Ŀ��� �巡���� ������ �����Ͽ� ���ο� �ؽ��� ����
        Texture2D editedTexture = new Texture2D((int)_dragRect.width, (int)_dragRect.height);
        Color[] pixels = originalTexture.GetPixels((int)_dragRect.x, (int)_dragRect.y, (int)_dragRect.width, (int)_dragRect.height);
        editedTexture.SetPixels(pixels);
        editedTexture.Apply();


        dragIndicator.color = Color.white;
        // ���ο� �ؽ��ĸ� ��� �������� ����
        dragIndicator.texture = editedTexture;


        // �巡���� ������ �ȼ��� �������� ä���
        //Color[] transparentPixels = new Color[(int)_dragRect.width * (int)_dragRect.height];
        //for (int i = 0; i < transparentPixels.Length; i++)
        //{
        //    transparentPixels[i] = Color.clear; // ������ �������� ä���
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

        // �巡�� ���� ������ ���� ���콺 ��ġ�� ĵ������ ���� ��ǥ�� ��ȯ
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, dragStartPosition, null, out localStartPos);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, currentMousePosition, null, out localEndPos);

        localStartPos = dragStartPosition;
        localEndPos =currentMousePosition;
        localStartPos += new Vector2(350f, 350f);
        localEndPos += new Vector2(350f, 350f);
        // �巡���� ������ Rect ���
        float xMin = Mathf.Min(Mathf.RoundToInt( localStartPos.x), Mathf.RoundToInt(localEndPos.x));
        float yMin = Mathf.Min(Mathf.RoundToInt(localStartPos.y), Mathf.RoundToInt(localEndPos.y));
        float width = Mathf.Abs(localEndPos.x - localStartPos.x);
        float height = Mathf.Abs(localEndPos.y - localStartPos.y);

        return new Rect(xMin, yMin, width, height);
    }
    Texture2D RenderTextureToTexture2D(RenderTexture rt)
    {
        // ���� Ȱ�� ���� �ؽ�ó�� �����ϰ� ���ο� RenderTexture�� ����
        // ���ο� Texture2D ���� �� ReadPixels�� �ȼ� ����
        RenderTexture.active = rt;
        Texture2D texture = new Texture2D(rt.width, rt.height);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // ������ Ȱ�� ���� �ؽ�ó�� ����
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