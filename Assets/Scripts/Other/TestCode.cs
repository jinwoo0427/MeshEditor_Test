using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    public Shader customShader; // 사용할 쉐이더를 Unity 에디터에서 설정
    public Mesh mesh;
    void Start()
    {
        // 새로운 머테리얼 생성
        Material newMaterial = new Material(customShader);
        newMaterial.SetFloat("_Scale", 3f);
        // 오브젝트에 새로 만든 머테리얼 할당
        transform.GetComponent<Renderer>().material = newMaterial;
    }

    void Update()
    {
        // 게임뷰는 안보이게 하는 설정
        //if( (Camera.current.gameObject.hideFlags & SceneCameraHideFlags) != SceneCameraHideFlags || Camera.current.name != "SceneCamera" )
        //	return;

        Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
    }
}
