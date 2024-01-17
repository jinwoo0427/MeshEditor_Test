using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UIUVLines : MonoBehaviour
{
    private UILineRenderer lineRenderer;

    public Mesh mesh;
    private Vector2 offset;
    private List<Vector2> uvPoints = new List<Vector2>();
    void Start()
    {
        lineRenderer = GetComponent<UILineRenderer>();

    }
    public void SetMesh(Mesh mesh, Vector2 offsetPos)
    {
        this.mesh = mesh;
        offset = offsetPos;
    }

    public void RenderLines()
    {

        Vector2[] uv0 = mesh.uv;
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            Vector2 point = transform.InverseTransformDirection( uv0[(int)mesh.triangles[i]] );
            point *= offset;
            point -= new Vector2(offset.x / 2, offset.y / 2);
            points.Add(point);
        }
        lineRenderer.Points = null;
        lineRenderer.Points = points.ToArray();
    }

}
