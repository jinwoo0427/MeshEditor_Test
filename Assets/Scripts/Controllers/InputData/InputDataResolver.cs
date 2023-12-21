using XDPaint.Controllers.InputData.Base;
using XDPaint.Core;
using XDPaint.Tools;

namespace XDPaint.Controllers.InputData
{
    public class InputDataResolver
    {
        public BaseInputData Resolve(ObjectComponentType objectComponentType, bool isModify = false)
        {
            if (isModify)
            {
                return new InputDataMeshModify();
            }
            if (objectComponentType == ObjectComponentType.MeshFilter || objectComponentType == ObjectComponentType.SkinnedMeshRenderer)
            {
                return new InputDataMesh();
            }
            if (objectComponentType == ObjectComponentType.RawImage)
            {
                return new InputDataCanvas();
            }
            return new InputDataDefault();
        }
    }
}