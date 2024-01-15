using System;
using System.Collections.Generic;
using GetampedPaint.Tools.Raycast.Base;

namespace GetampedPaint.Controllers.InputData
{
    public class InputRequest
    {
        public RaycastRequestContainer RequestContainer;
        public List<Action<RaycastRequestContainer>> Callbacks;
    }
}