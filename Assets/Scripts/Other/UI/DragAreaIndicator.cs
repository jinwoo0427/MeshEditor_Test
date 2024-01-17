using RuntimeHandle;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GetampedPaint.Controllers;
using GetampedPaint.Core;
using GetampedPaint.Core.Layers;
using GetampedPaint.Tools.Images.Base;
using GetampedPaint.Utils;


public class DragAreaIndicator : MonoBehaviour
{
    public RawImage dragIndicator; // �巡�� ������ ��Ÿ���� �̹���
    public RectTransform dragRectTransform;
    public RectTransform dragArea; // Ư�� ������ ��Ÿ���� RectTransform

    public GameObject DragHandleObj;
    public List<DragHandle> DragHandles ;

    private RawImage paintBoard; // �巡�� ������ ��Ÿ���� �̹���
    private RectTransform dragRect; // �巡�� ���� ���� ����
    private Vector2 dragStartPosition; // �巡�� ���� ����
    private Vector2 currentMousePosition; // ���� ���콺 ��ġ
    private Vector2 lastMousePosition;
    private bool isDragMoving = false;
    private bool isDragging = false;
    private bool isDragComplete = false;
    public bool isEnable = false;

    private Vector3 bottomLeft;
    private Vector3 topRight;
    private Texture2D originalTexture;

    public Texture2D expansionCursor;
    public Texture2D moveCursor;
    
    
    void Start()
    {
        paintBoard = dragArea.GetComponent<RawImage>();

        Init();
        Vector3[] corners = new Vector3[4];

        dragArea.GetLocalCorners(corners);

        bottomLeft = corners[0];
        topRight = corners[2];

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    public void Init()
    {
        isDragMoving = false;
        isDragging = false;
        isDragComplete = false;
        isEnable = false;
        dragRect = dragRectTransform;
        dragIndicator.gameObject.SetActive(false);
        DragHandleObj.gameObject.SetActive(false);

    }

    void Update()
    {

        if (PaintController.Instance.GetCurPaintManager().Tool == GetampedPaint.Core.PaintTool.Selection )
        {
            if (isDragComplete == false)
            {
                HandleDragInput();

            }
            else
            {
                DragMoveInput();
            }
        }
        else if (PaintController.Instance.GetCurPaintManager().Tool != GetampedPaint.Core.PaintTool.Selection)
        {
            Init();
        }

    }
    public void DeleteDragImage()
    {
        Init();
    }
    public void AddEditTexture()
    {
        isEnable = false;
        Texture2D addTexture = dragIndicator.texture as Texture2D;

        if (addTexture != null)
        {
            Vector3[] corners = new Vector3[4];
            dragRect.GetWorldCorners(corners);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, corners[0], null, out Vector2 point1);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, corners[2], null, out Vector2 point2);

            Rect _dragRect = GetDragRect(point1, point2);

            // paintBoard.uvRect�� ������ ����Ͽ� �ؽ�ó ũ�� ���
            float uvWidth = paintBoard.uvRect.width;
            float uvHeight = paintBoard.uvRect.height;
            // paintBoard.uvRect�� ������ ����Ͽ� ��ġ ���
            float uvX = paintBoard.uvRect.x;
            float uvY = paintBoard.uvRect.y;

            int newWidth = (int)(_dragRect.width * uvWidth);
            int newHeight = (int)(_dragRect.height * uvHeight);

            if (newWidth <= 0 || newHeight <= 0)
                return;
            Texture2D tex = ResizeTexture(addTexture, newWidth, newHeight);
            // �巡�� ũ�� * �巡�� ��ġ ���� + ������ ��ġ = ���� �巡�� ��ġ
            int dragX = (int)((originalTexture.width * uvWidth) * (_dragRect.x / originalTexture.width) + (uvX * originalTexture.width));
            int dragY = (int)((originalTexture.height * uvHeight) * (_dragRect.y / originalTexture.height) + (uvY * originalTexture.height));

            Rect adjustDragRect = new Rect(dragX, dragY, newWidth, newHeight);
            Vector2 dragPos = new Vector2(dragX, dragY);
            // ���ο� Texture2D�� ���̾ �߰�
            PaintController.Instance.GetCurPaintManager().LayersController.AddLayerImage(tex, adjustDragRect, dragPos);
        }
    }
    // �ؽ�ó ũ�� ���� �� �ȼ� ��� ���� ���� �Լ�
    Texture2D ResizeTexture(Texture2D sourceTexture, int newWidth, int newHeight)
    {
        Texture2D newTexture = new Texture2D(newWidth, newHeight);

        // ���� �ؽ�ó�� �ȼ� ������ ��������
        Color[] sourcePixels = sourceTexture.GetPixels();

        // �ȼ� ũ�� ���� ���
        float xRatio = (float)sourceTexture.width / newWidth;
        float yRatio = (float)sourceTexture.height / newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // ���ο� �ȼ� ��ġ ���
                int sourceX = Mathf.FloorToInt(x * xRatio);
                int sourceY = Mathf.FloorToInt(y * yRatio);

                // �ֺ� �� ���� �ȼ��� ��� ���� ���
                Color averageColor = AverageColor(sourcePixels, sourceX, sourceY, sourceTexture.width, sourceTexture.height);

                // ���ο� �ؽ�ó�� �ȼ� ���� ����
                newTexture.SetPixel(x, y, averageColor);
            }
        }
        newTexture.Apply();

        return newTexture;
    }

    // �ֺ� �� ���� �ȼ��� ��� ���� ��� �Լ�
    Color AverageColor(Color[] pixels, int x, int y, int textureWidth, int textureHeight)
    {
        Color sumColor = Color.clear;
        int count = 0;

        for (int yOffset = -1; yOffset <= 1; yOffset++)
        {
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                int neighborX = Mathf.Clamp(x + xOffset, 0, textureWidth - 1);
                int neighborY = Mathf.Clamp(y + yOffset, 0, textureHeight - 1);

                int index = neighborY * textureWidth + neighborX;

                // �ش� �ȼ��� �������� ���� ��쿡�� ��꿡 ����
                if (pixels[index].a > 0f)
                {
                    sumColor += pixels[index];
                    count++;
                }
            }
        }

        // �߰� ���� ���
        Color averageColor = count > 0 ? sumColor / count : Color.clear;

        return averageColor;
    }
    public void DragMoveInput()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteDragImage();
            return;
        }

        foreach (var h in DragHandles)
        {
            if (h.IndragHandle)
            {
                isDragMoving = false;
                return;
            }
        }

        if (!IsMouseInDragArea(dragRect) && Input.GetMouseButtonDown(0))
        {
            AddEditTexture();
            Init();
            return;
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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out Vector2 mousePos);

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
        isEnable = true;
        isDragging = true;
        dragIndicator.texture = null;
        dragIndicator.color = new Color(0f, 0f, 0f, 0.4f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out dragStartPosition);
        
        dragRect.anchoredPosition = dragStartPosition;
        dragRect.sizeDelta = Vector2.zero;
        dragIndicator.gameObject.SetActive(true);

    }
    public void UpdateDrag()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dragArea, Input.mousePosition, null, out currentMousePosition);

        //Debug.Log(dragStartPosition + " : " + currentMousePosition);

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
        Rect _dragRect = GetDragRect(dragStartPosition, currentMousePosition);

        var curPM = PaintController.Instance.GetCurPaintManager();
        RenderTexture tex = curPM.LayersController.ActiveLayer.RenderTexture;
        originalTexture = tex.GetTexture2D();


        // paintBoard.uvRect�� ������ ����Ͽ� ������ ��ġ �� ũ�� ���
        float uvX = paintBoard.uvRect.x;
        float uvY = paintBoard.uvRect.y;
        float uvWidth = paintBoard.uvRect.width;
        float uvHeight = paintBoard.uvRect.height;

        int dragX = (int)((originalTexture.width * uvWidth) * (_dragRect.x / originalTexture.width) + (uvX * originalTexture.width));
        int dragY = (int)((originalTexture.height * uvHeight) * (_dragRect.y / originalTexture.height) + (uvY * originalTexture.height));

        // �ؽ�ó���� �巡���� ������ �����Ͽ� ���ο� �ؽ�ó ����
        Texture2D editedTexture = new Texture2D((int)(_dragRect.width * uvWidth), (int)(_dragRect.height * uvHeight));
        Color[] pixels = originalTexture.GetPixels(
            dragX,
            dragY,
            (int)(_dragRect.width * uvWidth),
            (int)(_dragRect.height * uvHeight)
        );
        //Debug.Log(_dragRect);

        editedTexture.SetPixels(pixels);
        editedTexture.filterMode = FilterMode.Point;
        editedTexture.Apply();

        // �巡���� ������ �ȼ��� �������� ä���
         Color[] transparentPixels = new Color[editedTexture.GetPixels().Length];
        for (int i = 0; i < transparentPixels.Length; i++)
        {
            transparentPixels[i] = Color.clear; // ������ �������� ä���
        }

        originalTexture.SetPixels(
            dragX,
            dragY,
            (int)(_dragRect.width * uvWidth),
            (int)(_dragRect.height * uvHeight),
            transparentPixels
        );

        originalTexture.Apply();

        dragIndicator.color = Color.white;
        dragIndicator.texture = editedTexture;


        Graphics.Blit(originalTexture, tex);
        ApplyRender();
    }

    private void ApplyRender()
    {
        var Data = PaintController.Instance.GetCurPaintManager().GetPaintData();
        Data.Material.SetTexture(Constants.PaintShader.PaintTexture, Data.LayersController.ActiveLayer.RenderTexture);
        Data.CommandBuilder.Clear().SetRenderTarget(Data.TexturesHelper.GetTarget(RenderTarget.ActiveLayerTemp)).ClearRenderTarget().
            DrawMesh(Data.QuadMesh, Data.Material, PaintPass.Paint, PaintPass.Erase).Execute();
        Data.Material.SetTexture(Constants.PaintShader.PaintTexture, Data.TexturesHelper.GetTexture(RenderTarget.ActiveLayerTemp));
    }

    private Rect GetDragRect(Vector2 dragStartPosition, Vector2 currentMousePosition)
    {
        Vector2 localStartPos, localEndPos;

        localStartPos = dragStartPosition;
        localEndPos =currentMousePosition;
        localStartPos += new Vector2(dragArea.rect.width / 2, dragArea.rect.height / 2);
        localEndPos += new Vector2(dragArea.rect.width / 2, dragArea.rect.height / 2);
        // �巡���� ������ Rect ���
        float xMin = Mathf.Min(Mathf.RoundToInt( localStartPos.x), Mathf.RoundToInt(localEndPos.x));
        float yMin = Mathf.Min(Mathf.RoundToInt(localStartPos.y), Mathf.RoundToInt(localEndPos.y));
        float width = Mathf.Abs(localEndPos.x - localStartPos.x);
        float height = Mathf.Abs(localEndPos.y - localStartPos.y);

        return new Rect(xMin, yMin, width , height );
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