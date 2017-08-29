using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enyim.Collections;
using OSGeo.OGR;
using System.Linq;

public class utils{
    public static Enyim.Collections.Envelope Rect2ECEnvlope(Rect rc)
    {
        return new Enyim.Collections.Envelope(rc.xMin, rc.yMin, rc.xMax, rc.yMax);
    }

    public static Rect ECEnvlope2Rect(Enyim.Collections.Envelope rc)
    {
        Rect r = new Rect();
        r.xMin = (float)rc.X1;
        r.yMin = (float)rc.Y1;
        r.xMax = (float)rc.X2;
        r.yMax = (float)rc.Y2;
        return r;
    }

    public static Rect CreateRectFromPoints(List<Vector2> lst)
    {
        Rect rc = new Rect();
        float[] arrx = new float[lst.Count];
        float[] arry = new float[lst.Count];
        for (int i = 0; i < lst.Count; i++)
        {
            arrx[i] = lst[i].x;
            arry[i] = lst[i].y;
        }

        rc.xMin = arrx.Min();
        rc.xMax = arrx.Max();
        rc.yMin = arry.Min();
        rc.yMax = arry.Max();
        return rc;
    }
       

    public static bool PositionInView(Vector3 pos)
    {
        return pos.x < Screen.width
            && pos.x > 0
            && pos.y < Screen.height
            && pos.y > 0;
    }
}

public class Vector2D
{
    public Vector2D()
    {
        x = 0;
        y = 0;
    }
    public double x;
    public double y;

    public override string ToString()
    {
        return string.Format("x: {0}, y: {1}", x, y);
    }
}
