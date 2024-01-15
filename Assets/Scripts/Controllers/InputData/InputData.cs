using UnityEngine;
using GetampedPaint.Tools.Raycast.Data;

namespace GetampedPaint.Controllers.InputData
{
    public class InputData
    {
        public Ray? Ray; // ? : �� üũ
        public RaycastData RaycastData;
        public Vector3 Position;
        public Vector3? PreviousPosition;
    }
}