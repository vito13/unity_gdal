using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class GisPointTool : GisOperatingTool
{
    protected override void init()
    {
        line = new VectorLine(ToString(), new List<Vector2>(), null, 5.0f, LineType.Points);
        line.SetColor(Color.white);
        type = OperatingToolType.GISPoint;
        Reset();
    }

    public override void OnButtonDown()
    {
        line.points2.Add(Input.mousePosition);
        line.Draw();
    }
  
    public override void Reset()
    {
        line.points2.Clear();
    }
}

