using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    public Shader customShader; // ����� ���̴��� Unity �����Ϳ��� ����
    public Mesh mesh;
    void Start()
    {
        // ���ο� ���׸��� ����
        Material newMaterial = new Material(customShader);
        newMaterial.SetFloat("_Scale", 3f);
        // ������Ʈ�� ���� ���� ���׸��� �Ҵ�
        transform.GetComponent<Renderer>().material = newMaterial;
    }

    void Update()
    {
        // ���Ӻ�� �Ⱥ��̰� �ϴ� ����
        //if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
        //	return;

        Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
    }
}
