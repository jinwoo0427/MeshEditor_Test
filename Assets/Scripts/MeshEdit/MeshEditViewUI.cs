using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class MeshEditViewUI : MonoBehaviour
{
    public MeshFilter meshFilter;
    public RectTransform pointPrefab;
    public Camera cam;
    private RectTransform[] pointUIs;

    public Transform modelTransform;

    public UILineRenderer lineRenderer;

    public List<Transform> posList;
    void Start()
    {
        if (meshFilter == null || pointPrefab == null)
        {
            Debug.LogError("MeshFilter or Point Prefab is missing!");
            return;
        }

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        // 삼각형 꼭지점의 월드 좌표를 스크린 좌표로 변환하여 UI에 표시
        pointUIs = new RectTransform[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            RectTransform pointUI = Instantiate(pointPrefab, transform);
            pointUIs[i] = pointUI;
        }
        UpdateDot();
        //UpdateLineRenderer();
    }
    private void Update()
    {
        UpdateDot();
    }
    bool IsInCameraView(Vector3 screenPos)
    {
        float boundingBoxSize = 1920f; // 적절한 크기로 조절해보세요
        Bounds bounds = new Bounds(screenPos, Vector3.one * boundingBoxSize);

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }
    void UpdateDot()
    {
        if (meshFilter == null || pointPrefab == null)
            return;

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = modelTransform.TransformPoint(vertices[i]);
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
            //Debug.Log(worldPos + " " + screenPos);
            Debug.Log(IsInCameraView(screenPos));
            if (IsInCameraView(screenPos))
            {
                pointUIs[i].gameObject.SetActive(true);
                pointUIs[i].position = screenPos;
            }
            else
            {
                pointUIs[i].gameObject.SetActive(false);
            }
        }
        //UpdateLineRenderer(mesh.triangles);
    }
    void UpdateLineRenderer(int[] triangles)
    {
        if (meshFilter == null)
            return;
        List<Vector2> points = new List<Vector2>();
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 worldPos1 = modelTransform.TransformPoint(vertices[triangles[i]]);
            Vector3 worldPos2 = modelTransform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 worldPos3 = modelTransform.TransformPoint(vertices[triangles[i + 2]]);

            Vector3 screenPos1 = cam.WorldToScreenPoint(worldPos1);
            Vector3 screenPos2 = cam.WorldToScreenPoint(worldPos2);
            Vector3 screenPos3 = cam.WorldToScreenPoint(worldPos3);

            if (IsInCameraView(screenPos1) && IsInCameraView(screenPos2) && IsInCameraView(screenPos3))
            {
                points.Add(screenPos1);
                points.Add(screenPos2);
                points.Add(screenPos3);
            }
            if (i + 3 == triangles.Length)
            {
                points.Add(screenPos1);

            }
        }

        lineRenderer.Points = null;
        lineRenderer.Points = points.ToArray();
    }
    void OnDrawGizmos()
    {
        if (cam != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = cam.transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
        }
    }
}
