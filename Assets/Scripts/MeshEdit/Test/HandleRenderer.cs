using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleRenderer : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    void OnDestroy()
    {
        if (mesh) DestroyImmediate(mesh);
        if (material) DestroyImmediate(material);
    }

    void LateUpdate()
    {
        // ���Ӻ�� �Ⱥ��̰� �ϴ� ����
        //if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
        //	return;
        Mesh msh = mesh;
        Material mat = material;

        if (mat == null || msh == null || !material.SetPass(0))
            return;

        Graphics.DrawMeshNow(msh, transform.localToWorldMatrix);
    }
}
