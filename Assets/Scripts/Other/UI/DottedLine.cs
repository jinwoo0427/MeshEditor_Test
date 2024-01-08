using UnityEngine;

public class DottedLine : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float dotSpacing = 0.1f; // 점 간격
    public Transform startPoint;
    public Transform endPoint;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 0; // 초기화
        DrawDottedLine();
    }

    void DrawDottedLine()
    {
        Vector3[] linePositions = CalculateDottedLinePositions();
        lineRenderer.positionCount = linePositions.Length;
        lineRenderer.SetPositions(linePositions);
    }

    Vector3[] CalculateDottedLinePositions()
    {
        Vector3[] positions = new Vector3[CalculateDotCount()];
        Vector3 direction = (endPoint.position - startPoint.position).normalized;

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = startPoint.position + direction * (i * dotSpacing);
        }

        return positions;
    }

    int CalculateDotCount()
    {
        float lineLength = Vector3.Distance(startPoint.position, endPoint.position);
        return Mathf.CeilToInt(lineLength / dotSpacing);
    }
}