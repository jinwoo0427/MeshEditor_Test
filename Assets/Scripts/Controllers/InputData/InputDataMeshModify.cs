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
    public override void OnHover(int fingerId, Vector3 position)
    {
        base.OnHover(fingerId, position);
    }


    protected override void OnHoverFailed(int fingerId)
    {
        base.OnHoverFailed(fingerId);
    }

    public override void OnDown(int fingerId, Vector3 position, float pressure = 1)
    {
        base.OnDown(fingerId, position, pressure);
    }


    protected override void OnDownFailed(int fingerId, Vector3 position, float pressure = 1)
    {
        base.OnDownFailed(fingerId, position, pressure);
    }

    public override void OnPress(int fingerId, Vector3 position, float pressure = 1)
    {
        base.OnPress(fingerId, position, pressure);
    }



    protected override void OnPressFailed(int fingerId, Vector3 position, float pressure = 1)
    {
        base.OnPressFailed(fingerId, position, pressure);
    }

    public override void OnUp(int fingerId, Vector3 position)
    {
        base.OnUp(fingerId, position);
    }
    protected override void OnHoverSuccess(int fingerId, Vector3 position, RaycastData raycast)
    {
        var data = InputData[fingerId];
        data.Position = position;
        data.Ray = Camera.ScreenPointToRay(position);
        

        data.PreviousPosition = position;
    }


    protected override void OnDownSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
    {

    }


    protected override void OnPressSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
    {
       
    }

    protected override void OnUpSuccessInvoke(int fingerId, Vector3 position)
    {
        
    }

    public override void DoDispose()
    {
        base.DoDispose();
    }

    
}
