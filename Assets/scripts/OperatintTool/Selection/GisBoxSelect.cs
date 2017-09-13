using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class GisBoxSelect : GisOperatingTool
{
    protected override void init()
    {
        type = OperatingToolType.GISBoxSelect;
        Reset();
    }

    public override void OnButtonDown()
    {
        line = new VectorLine("Selection", new List<Vector2>(5), null, 1.0f, LineType.Continuous);
        line.capLength = 0.5f;
        line.SetColor(Color.white);
        originalPos = Input.mousePosition;
    }
    public override void OnButton()
    {
        line.MakeRect(originalPos, Input.mousePosition);
        line.Draw();
    }
    public override void OnButtonUp()
    {
        if (null == line)
        {
            return;
        }
        Vector2[] arr = new Vector2[line.points2.Count];
        for (int i = 0; i < line.points2.Count; i++)
        {
            arr[i] = line.points2[i];
        }
        Send("SpatialQuery", arr);
        VectorLine.Destroy(ref line);
    }
}
