using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUVLines : MonoBehaviour
{
    private LineRenderer lineRenderer;
    
    public GameObject lineContainer;
    public Mesh mesh;
    private List<LineRenderer> uvLines = new List<LineRenderer>();

    void Start()
    {
        
    }
    public void SetMesh(Mesh mesh)
    {
        this.mesh = mesh;
    }

    public void RenderLines()
    {

        foreach (var x in uvLines)
        {
            Destroy(x.gameObject);
        }


        uvLines.Clear();

        Vector2[] uv0 = mesh.uv;
        Vector2[] triangle = new Vector2[3];
        int tri = 0;
        for (int i = 0; i < mesh.triangles.Length; i++)
        {
            if (tri >= 3)
            {
                CreateUVLine(triangle[0], triangle[1], triangle[2]);

                tri = 0;
            }
            triangle[tri++] = uv0[mesh.triangles[i]];
        }
    }
    private void CreateUVLine(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        //var line = Instantiate(lineRenderer, lineContainer.transform);

        //line.positionCount = 4;

        //line.SetPosition(0, v1);
        //line.SetPosition(1, v2);
        //line.SetPosition(2, v3);
        //line.SetPosition(3, v1);

        //uvLines.Add(line);
    }
}
