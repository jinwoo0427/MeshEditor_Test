using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GetampedPaint.Core;
using GetampedPaint.Core.PaintObject.Base;
public class BaseMeshModifyData 
{
    public readonly LineData LineData;
    public Vector3 PreviousEditPosition;
    public Vector2? ScreenPosition { get; set; }
    public Vector3? LocalPosition { get; set; }
    public Vector3? EditPosition { get; set; }
    public bool InBounds { get; set; }
    public bool IsEditing { get; set; }
    public bool IsEditingDone { get; set; }

    public BaseMeshModifyData(bool useExtraDataForLines)
    {
        var lineElements = useExtraDataForLines ? 3 : 1;
        LineData = new LineData(lineElements);
    }

}
