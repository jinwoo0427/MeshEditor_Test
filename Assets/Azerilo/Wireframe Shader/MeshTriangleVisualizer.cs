using UnityEngine;

[ExecuteInEditMode]
public class MeshTriangleVisualizer : MonoBehaviour
{
    public Material vertexVisualizationMaterial;

    void OnRenderObject()
    {
        if (vertexVisualizationMaterial != null)
        {
            Debug.Log("dsalkjfasdfjlkasd;fjlas;fl");
            vertexVisualizationMaterial.SetPass(0);

            Graphics.DrawMeshNow(GetComponent<MeshFilter>().sharedMesh, transform.localToWorldMatrix);
        }
    }
}
