using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class GisPolylineTool : GisOperatingTool
{
    protected override void init()
    {
        line = new VectorLine(ToString(), new List<Vector2>(), null, 1.0f, LineType.Continuous);
        line.SetColor(Color.white);
        type = OperatingToolType.GISPolyline;
        Reset();
    }

    public override void OnButtonDown()
    {
        line.points2[line.points2.Count - 1] = Input.mousePosition;
        line.points2.Add(Vector2.zero);
    }

    public override void OnMove()
    {
        if (line.points2.Count >= 2)
        {
            line.points2[line.points2.Count - 1] = Input.mousePosition;
            line.Draw();
        }
    }

    public override void OnDblClk()
    {
        if (line.points2.Count >= 2)
        {
            line.points2[line.points2.Count - 1] = Input.mousePosition;
            line.Draw();
        }
        Reset();
    }

    public override void Reset()
    {
        line.points2.Clear();
        line.points2.Add(Vector2.zero);
    }
}
