using System;
using System.Collections.Generic;
using XDPaint.Tools.Raycast.Base;

namespace XDPaint.Controllers.InputData
{
    public class InputRequest
    {
        public RaycastRequestContainer RequestContainer;
        public List<Action<RaycastRequestContainer>> Callbacks;
    }
}