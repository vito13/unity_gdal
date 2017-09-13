using System.Collections.Generic;
using UnityEngine;
using Vectrosity;


public class GisCircleSelect : GisOperatingTool
{
    protected VectorLine lr;
    protected override void init()
    {
        type = OperatingToolType.GISBoxSelect;
        Reset();
    }

    public override void OnButtonDown()
    {
        lr = new VectorLine("line", new List<Vector2>(2), null, 1.0f, LineType.Continuous);
        line = new VectorLine("Selection", new List<Vector2>(361), null, 1.0f, LineType.Continuous);
        lr.capLength = 0.5f;
        line.capLength = 0.5f;
        lr.SetColor(Color.white);
        line.SetColor(Color.white);
        originalPos = Input.mousePosition;
        lr.points2[0] = originalPos;
    }
    public override void OnButton()
    {
        float radius = Vector2.Distance(new Vector2(Input.mousePosition.x, Input.mousePosition.y), originalPos);
        line.MakeCircle(originalPos, radius, 360);
        line.Draw();
        lr.points2[1] = Input.mousePosition;
        lr.Draw();
    }
    public override void OnButtonUp()
    {
        if (null == line)
        {
            return;
        }
        Vector2[] arr = new Vector2[360];
        for (int i = 0; i < 360; i++)
        {
            arr[i] = line.points2[i];
        }
        Send("SpatialQuery", arr);
        VectorLine.Destroy(ref line);
        VectorLine.Destroy(ref lr);
    }
}
