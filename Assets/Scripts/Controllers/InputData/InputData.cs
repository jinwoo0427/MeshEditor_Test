using UnityEngine;
using GetampedPaint.Tools.Raycast.Data;

namespace GetampedPaint.Controllers.InputData
{
    public class InputData
    {
        public Ray? Ray; // ? : ³Î Ã¼Å©
        public RaycastData RaycastData;
        public Vector3 Position;
        public Vector3? PreviousPosition;
    }
}