using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class TestCode : MonoBehaviour
{
    public Shader customShader; // 사용할 쉐이더를 Unity 에디터에서 설정
    public Mesh   mesh;
    public Mesh   originalmesh;
    [SerializeField]
    private Vector3[] verticesInWorldSpace;
    [SerializeField]
    private Material material;

    void Start()
    {
        // 새로운 머테리얼 생성
        Material newMaterial = new Material(customShader);
        newMaterial.SetFloat("_Scale", 3f);


        Mesh clonedMesh = new Mesh();
        Copy(clonedMesh, originalmesh);

        clonedMesh.RecalculateBounds();
        mesh = clonedMesh;
        //transform.GetComponent<MeshFilter>().sharedMesh = clonedMesh;
        material = newMaterial;

        //transform.GetComponent<MeshRenderer>().material = material;

        //CacheMeshValues();
        Vector3[] selectV = new Vector3[0];
        //selectV[0] = mesh.vertices[0];
        MakeVertexSelectionMesh(ref mesh, transform.GetComponent<MeshFilter>().mesh.vertices, selectV);
        //transform.GetComponent<MeshFilter>().mesh = mesh;
        CacheMeshValues();
    }
    void Update()
    {

        Mesh msh = mesh;
        Material mat = material;
        if (mat == null || msh == null || !material.SetPass(0))
        {
            Debug.Log("머가 문제니");
            return;
        }


        Graphics.DrawMeshNow(msh, transform.localToWorldMatrix);
    }

    // 메쉬 카피 해주는 넘
    public static void Copy(Mesh destMesh, Mesh src)
    {
        destMesh.Clear();

        destMesh.vertices = src.vertices;
        destMesh.uv = src.uv;
        destMesh.uv2 = src.uv2;
        destMesh.normals = src.normals;
        destMesh.tangents = src.tangents;
        destMesh.boneWeights = src.boneWeights;
        destMesh.colors = src.colors;
        destMesh.colors32 = src.colors32;
        destMesh.bindposes = src.bindposes;

        destMesh.subMeshCount = src.subMeshCount;
        for (int i = 0; i < src.subMeshCount; i++)
            destMesh.SetIndices(src.GetIndices(i), src.GetTopology(i), i);
    }
    // 메쉬의 로컬 버텍스 좌표들을 월드로 변환시켜주기
    public void CacheMeshValues()
    {
        Vector3[] v = mesh.vertices;
        int vc = v.Length;
        Debug.Log(vc);
        verticesInWorldSpace = new Vector3[vc];
        Matrix4x4 matrix = transform.localToWorldMatrix;

        for (int i = 0; i < vc; i++)
            verticesInWorldSpace[i] = matrix.MultiplyPoint3x4(v[i]);
    }
    // 버텍스 점 메쉬들 생성해주는 곳
    public static void MakeVertexSelectionMesh(ref Mesh mesh, Vector3[] vertices, Vector3[] selected)
    {
        int vl = vertices.Length;
        int sl = selected.Length;

        Debug.Log(sl);

        Vector3[] v = new Vector3[vl + sl];
        System.Array.Copy(vertices, 0, v, 0, vl); //v배열에 카피
        System.Array.Copy(selected, 0, v, vl, sl);

        Vector3[] t_billboards = new Vector3[v.Length * 4];
        Vector3[] t_nrm = new Vector3[v.Length * 4];
        Vector2[] t_uvs = new Vector2[v.Length * 4];
        Vector2[] t_uv2 = new Vector2[v.Length * 4];
        Color32[] t_col = new Color32[v.Length * 4];
        int[] t_tris = new int[v.Length * 6];

        int n = 0;
        int t = 0;

        Vector3 up = Vector3.up;
        Vector3 right = Vector3.right;

        for (int i = 0; i < vl; i++)
        {
            t_billboards[t + 0] = v[i];//-up-right;
            t_billboards[t + 1] = v[i];//-up+right;
            t_billboards[t + 2] = v[i];//+up-right;
            t_billboards[t + 3] = v[i];//+up+right;

            t_uvs[t + 0] = Vector3.zero;
            t_uvs[t + 1] = Vector3.right;
            t_uvs[t + 2] = Vector3.up;
            t_uvs[t + 3] = Vector3.one;

            t_uv2[t + 0] = -up - right;
            t_uv2[t + 1] = -up + right;
            t_uv2[t + 2] = up - right;
            t_uv2[t + 3] = up + right;

            t_nrm[t + 0] = Vector3.forward;
            t_nrm[t + 1] = Vector3.forward;
            t_nrm[t + 2] = Vector3.forward;
            t_nrm[t + 3] = Vector3.forward;

            t_tris[n + 0] = t + 0;
            t_tris[n + 1] = t + 1;
            t_tris[n + 2] = t + 2;
            t_tris[n + 3] = t + 1;
            t_tris[n + 4] = t + 3;
            t_tris[n + 5] = t + 2;

            t_col[t + 0] = (Color32)Color.white;
            t_col[t + 1] = (Color32)Color.white;
            t_col[t + 2] = (Color32)Color.white;
            t_col[t + 3] = (Color32)Color.white;

            t += 4;
            n += 6;
        }

        for (int i = vl; i < v.Length; i++)
        {
            t_billboards[t + 0] = v[i];
            t_billboards[t + 1] = v[i];
            t_billboards[t + 2] = v[i];
            t_billboards[t + 3] = v[i];

            t_uvs[t + 0] = Vector3.zero;
            t_uvs[t + 1] = Vector3.right;
            t_uvs[t + 2] = Vector3.up;
            t_uvs[t + 3] = Vector3.one;

            t_uv2[t + 0] = -up - right;
            t_uv2[t + 1] = -up + right;
            t_uv2[t + 2] = up - right;
            t_uv2[t + 3] = up + right;

            t_nrm[t + 0] = Vector3.forward;
            t_nrm[t + 1] = Vector3.forward;
            t_nrm[t + 2] = Vector3.forward;
            t_nrm[t + 3] = Vector3.forward;

            t_tris[n + 0] = t + 0;
            t_tris[n + 1] = t + 1;
            t_tris[n + 2] = t + 2;
            t_tris[n + 3] = t + 1;
            t_tris[n + 4] = t + 3;
            t_tris[n + 5] = t + 2;

            t_col[t + 0] = (Color32)Color.green;
            t_col[t + 1] = (Color32)Color.green;
            t_col[t + 2] = (Color32)Color.green;
            t_col[t + 3] = (Color32)Color.green;

            t_nrm[t].x = .1f;
            t_nrm[t + 1].x = .1f;
            t_nrm[t + 2].x = .1f;
            t_nrm[t + 3].x = .1f;

            t += 4;
            n += 6;
        }


        mesh.Clear();
        mesh.vertices = t_billboards;
        mesh.normals = t_nrm;
        mesh.uv = t_uvs;
        mesh.uv2 = t_uv2;
        mesh.colors32 = t_col;
        mesh.triangles = t_tris;
        Debug.Log(mesh.vertices.Length);
    }
}
