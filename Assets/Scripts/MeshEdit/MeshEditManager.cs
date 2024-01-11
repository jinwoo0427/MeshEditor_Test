using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshEditManager : MonoBehaviour
{
    public Mesh mesh;
    public MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;

        Debug.Log(mesh.vertexCount);
        Debug.Log(mesh.triangles.Length);
        for(int i = 0; i < mesh.vertices.Length; i++)
        {
            Gizmos.DrawCube( mesh.vertices[i] , Vector3.one);

        }
    }

    void Update()
    {
        
    }
}
