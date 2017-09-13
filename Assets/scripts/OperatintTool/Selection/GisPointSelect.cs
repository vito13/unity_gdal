using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GisPointSelect : GisOperatingTool
{
    protected override void init()
    {
        type = OperatingToolType.GISPointSelect;
        Reset();
    }
 
    public override void OnButtonUp()
    {
        Vector2[] arr = new Vector2[1];
        arr[0] = Input.mousePosition;
        Send("SpatialQuery", arr);
    }
}
