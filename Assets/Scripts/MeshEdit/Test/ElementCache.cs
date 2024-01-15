using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GetampedPaint.Tools.Raycast;

[Serializable]
public class ElementCache : ScriptableObject
{
    public ModifyMesh mesh;
    public List<int> indices { get { return _indices; } }
    public List<Edge> edges { get { return _edges; } }
    public List<Triangle> faces { get { return _faces; } }
    public List<int> allIndices { get { return _allIndices; } }
    public List<int> userIndices { get { return _userIndices; } }

    public Vector3[] verticesInWorldSpace;
    public Transform transform { get { return mesh != null ? mesh.transform : null; } }

    public int selectedUserVertexCount { get; private set; }

    [SerializeField] private List<int> _indices = new List<int>();
    [SerializeField] private List<Edge> _edges = new List<Edge>();
    [SerializeField] private List<Triangle> _faces = new List<Triangle>();
    [SerializeField] private List<int> _allIndices = new List<int>();
    [SerializeField] private List<int> _userIndices = new List<int>();

    public void SetFaces(IList<Triangle> faces)
    {
        this._faces = faces.ToList();
        this._edges = faces.SelectMany(x => x.GetEdges())
                            .ToSharedIndex(mesh.triangleLookup)
                            .Distinct()
                            .ToTriangleIndex(mesh.sharedTriangles)
                            .ToList();
        this._indices = faces.SelectMany(x => x.GetIndices()).ToList();


        CacheIndices();
    }

    public void SetEdges(IList<Edge> edges)
    {
        this._faces.Clear();
        this._edges = edges.ToList();
        this._indices = edges.ToIndices().ToList();

        CacheIndices();
    }

    
    public void SetIndices(IList<int> indices)
    {
        this._faces.Clear();
        this._edges.Clear();
        this._indices = indices.ToList();

        CacheIndices();
    }

    private void CacheIndices()
    {
        _allIndices = mesh.GetAllIndices(this.indices).Distinct().ToList();
        _userIndices = (List<int>)mesh.GetUserIndices(this.indices);
        selectedUserVertexCount = _userIndices.Count;
    }

    public void CacheMeshValues()
    {
        Vector3[] v = mesh.vertices;
        int vc = v.Length;

        verticesInWorldSpace = new Vector3[vc];
        Matrix4x4 matrix = mesh.transform.localToWorldMatrix;

        for (int i = 0; i < vc; i++)
            verticesInWorldSpace[i] = matrix.MultiplyPoint3x4(v[i]);
    }

    public void Clear()
    {
        indices.Clear();
        edges.Clear();
        faces.Clear();
        _allIndices.Clear();
        _userIndices.Clear();
        selectedUserVertexCount = 0;
    }

   
}
