using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDPaint.Controllers.InputData;
using XDPaint.Controllers;
using XDPaint;
using XDPaint.Controllers.InputData.Base;
using XDPaint.Tools.Raycast.Base;
using XDPaint.Tools.Raycast.Data;

public class InputDataMeshModify : BaseInputData
{

    protected InputData[] InputData;

    public override void Init(MeshModifyManager meshManagerInstance, Camera camera)
    {
        base.Init( meshManagerInstance, camera);
        InputData = new InputData[InputController.Instance.MaxTouchesCount];
        for (var i = 0; i < InputData.Length; i++)
        {
            InputData[i] = new InputData();
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        // 업데이트 마다 인풋 데이타들의 레이 및 데이타 초기화
        foreach (var data in InputData)
        {
            data.Ray = null;
            data.RaycastData = null;
        }
    }

    protected override void OnHoverSuccess(int fingerId, Vector3 position, RaycastData raycast)
    {

    }

    protected virtual void OnHoverSuccessEnd(RaycastRequestContainer request, int fingerId, Vector3 position)
    {
        
    }

    protected override void OnDownSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
    {

    }

    protected virtual void OnDownSuccessCallback(RaycastRequestContainer request, int fingerId, Vector3 position, float pressure = 1.0f)
    {
       
    }

    protected override void OnPressSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
    {
       
    }

    protected virtual void OnPressSuccessCallback(RaycastRequestContainer request, int fingerId, Vector3 position, float pressure = 1.0f)
    {

    }

    protected override void OnUpSuccessInvoke(int fingerId, Vector3 position)
    {
        
    }
}
