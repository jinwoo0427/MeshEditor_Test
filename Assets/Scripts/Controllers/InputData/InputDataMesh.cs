using UnityEngine;
using GetampedPaint.Controllers.InputData.Base;
using GetampedPaint.Tools.Raycast.Base;
using GetampedPaint.Tools.Raycast.Data;

namespace GetampedPaint.Controllers.InputData
{
    public class InputDataMesh : BaseInputData
    {
        protected InputData[] InputData;

        public override void Init(PaintManager paintManagerInstance, Camera camera)
        {
            base.Init(paintManagerInstance, camera);
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
            var data = InputData[fingerId];
            data.Position = position;
            data.Ray = Camera.ScreenPointToRay(position);
            RaycastController.Instance.RequestRaycast(PaintManager, data.Ray.Value, fingerId, data.PreviousPosition, position, container =>
            {
                OnHoverSuccessEnd(container, fingerId, position);
            });
            data.PreviousPosition = position;
        }

        protected virtual void OnHoverSuccessEnd(RaycastRequestContainer request, int fingerId, Vector3 position)
        {
            var data = InputData[fingerId];
            if (data.RaycastData == null)
            {
                data.RaycastData = RaycastController.Instance.TryGetRaycast(request);
            }
            
            if (data.RaycastData != null)
            {
                OnHoverSuccessHandlerInvoke(fingerId, position, data.RaycastData);
            }
            else
            {
                base.OnHoverFailed(fingerId);
            }
        }

        protected override void OnDownSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            //Debug.Log("Ondownsucces");
            var data = InputData[fingerId];
            data.Position = position;
            if (data.Ray == null)
            {
                data.Ray = Camera.ScreenPointToRay(position);
            }
            
            if (data.RaycastData == null)
            {
                RaycastController.Instance.RequestRaycast(PaintManager, data.Ray.Value, fingerId, data.PreviousPosition, position, container =>
                {
                    OnDownSuccessCallback(container, fingerId, position, pressure);
                });
                data.PreviousPosition = position;
            }
        }

        protected virtual void OnDownSuccessCallback(RaycastRequestContainer request, int fingerId, Vector3 position, float pressure = 1.0f)
        {
            var data = InputData[fingerId];
            if (data.RaycastData == null)
            {
                data.RaycastData = RaycastController.Instance.TryGetRaycast(request);
            }

            if (data.RaycastData == null)
            {
                OnDownFailed(fingerId, position, pressure);
            }
            else
            {
                OnDownSuccessInvoke(fingerId, position, pressure, data.RaycastData);
            }
        }

        protected override void OnPressSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            var data = InputData[fingerId];
            data.Position = position;
            if (data.Ray == null)
            {
                data.Ray = Camera.ScreenPointToRay(position);
            }
                
            if (data.RaycastData == null)
            {
                RaycastController.Instance.RequestRaycast(PaintManager, data.Ray.Value, fingerId, data.PreviousPosition, position, container =>
                {
                    OnPressSuccessCallback(container, fingerId, position, pressure);
                });
                data.PreviousPosition = position;
            }
        }

        protected virtual void OnPressSuccessCallback(RaycastRequestContainer request, int fingerId, Vector3 position, float pressure = 1.0f)
        {
            var data = InputData[fingerId];
            if (data.RaycastData == null)
            {
                data.RaycastData = RaycastController.Instance.TryGetRaycast(request);
            }

            if (data.RaycastData == null)
            {
                OnPressFailed(fingerId, position, pressure);
            }
            else
            {
                OnPressSuccessInvoke(fingerId, position, pressure, data.RaycastData);
            }
        }

        protected override void OnUpSuccessInvoke(int fingerId, Vector3 position)
        {
            RaycastController.Instance.AddCallbackToRequest(PaintManager, fingerId, () => base.OnUpSuccessInvoke(fingerId, position));
        }
    }
}