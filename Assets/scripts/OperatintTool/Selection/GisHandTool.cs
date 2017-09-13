using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GisHandTool : GisOperatingTool
{
    GisViewer viewer;

    public GisHandTool(GisViewer v)
    {
        viewer = v;
    }

    protected override void init()
    {
        type = OperatingToolType.GISHand;
        Reset();
    }

    public override void OnButtonDown()
    {
        originalPos = Input.mousePosition;
    }
    public override void OnButton()
    {
        var dis = new Vector3(originalPos.x, originalPos.y, 0) - Input.mousePosition;
        viewer.Translate(new Vector2D(dis.x, dis.y));
        originalPos = Input.mousePosition;
    }
    public override void OnWheel(bool t)
    {
        viewer.Zooming(Input.mousePosition, t);
    }
}
