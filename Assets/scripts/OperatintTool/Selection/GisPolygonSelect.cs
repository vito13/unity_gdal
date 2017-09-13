using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class GisPolygonSelect : GisOperatingTool
{
    protected override void init()
    {
        type = OperatingToolType.GISPolygonSelect;
        Reset();
        line = null;
    }

    public override void OnButtonDown()
    {
        if (null == line)
        {
            line = new VectorLine("Selection", new List<Vector2>(), null, 1.0f, LineType.Continuous);
            line.capLength = 0.5f;
            line.SetColor(Color.white);
            originalPos = Input.mousePosition;
            line.points2.Add(Input.mousePosition);
        }
    }

    public override void OnMove()
    {
         if (null != line && line.points2.Count >= 2)
         {
             line.points2[line.points2.Count - 1] = Input.mousePosition;
             line.Draw();
         }
    }

    public override void OnDblClk()
    {
        if (null == line)
        {
            return;
        }
//         if (line.points2.Count < 3)
//         {
//             for (int i = 0; i < 3 - line.points2.Count; i++)
//             {
//                 line.points2.Add(line.points2[line.points2.Count - 1]);
//             }
//         }
        Vector2[] arr = new Vector2[line.points2.Count];
        for (int i = 0; i < line.points2.Count; i++)
        {
            arr[i] = line.points2[i];
        }
        Send("SpatialQuery", arr);
        VectorLine.Destroy(ref line);
        line = null;
    }
    public override void OnButtonUp()
    {
        if (null == line)
        {
            return;
        }
        line.points2.Add(Input.mousePosition);
    }
}
