using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enyim.Collections;
using OSGeo.OGR;
using System.Linq;
using System;

public class utils
{
    public static bool Intersects(Geometry host, Geometry other)
    {
        host.FlattenTo2D();
        other.FlattenTo2D();
        var r1 = host.Intersects(other);
        return r1;
    }

    public static bool Within(Geometry host, Geometry other)
    {
        host.FlattenTo2D();
        other.FlattenTo2D();
        var r1 = other.Within(host);
        return r1;
    }

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

    public static CPPOGREnvelope ECEnvlope2CPP(Enyim.Collections.Envelope rc)
    {
        return new CPPOGREnvelope(rc.X1, rc.Y1, rc.X2, rc.Y2);
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
        return pos.x <= Screen.width
            && pos.x >= 0
            && pos.y <= Screen.height
            && pos.y >= 0;
    }

    public static bool HaveClickMouseTwice(float offsetTime, ref float Timer, int keyNum)
    {
        if (Input.GetMouseButtonDown(keyNum))
        {
            return HaveExecuteTwiceAtTime(offsetTime, ref Timer);
        }
        return false;
    }

    static bool HaveExecuteTwiceAtTime(float offsetTime, ref float timer)
    {
        var d = Time.time - timer;
        if (d < offsetTime)
        {
            return true;
        }
        else
        {
            timer = Time.time;
            return false;
        }
    }

    public static Vector2D GetCameraWH()
    {
        Vector2D r = new Vector2D(0, 0);
        r.y = Camera.main.orthographicSize * 2;
        r.x = Camera.main.aspect * r.y;
        return r;
    }

    public static CPPOGREnvelope GetOrthographicCameraEnvelope()
    {
        var pos = Camera.main.transform.position;
        var wh = GetCameraWH();
        CPPOGREnvelope env = new CPPOGREnvelope();
        env.MinX = pos.x - wh.x * 0.5f;
        env.MaxX = env.MinX + wh.x;
        env.MinY = pos.y - wh.y * 0.5f;
        env.MaxY = env.MinY + wh.y;
        return env;
    }

    public static CPPOGREnvelope GetViewerEnvelope()
    {
        CPPOGREnvelope env = new CPPOGREnvelope();
        env.MinX = 0;
        env.MaxX = Screen.width;
        env.MinY = 0;
        env.MaxY = Screen.height;
        return env;
    }
}

public class Vector2D
{
    public Vector2D(double _x, double _y)
    {
        x = _x;
        y = _y;
    }
    public double x;
    public double y;

    public override string ToString()
    {
        return string.Format("x: {0:N4}, y: {1:N4}", x, y);
    }
}

public class CPPOGREnvelope
{
    public double MinX;
    public double MaxX;
    public double MinY;
    public double MaxY;
    public CPPOGREnvelope()
    {
        MinX = double.MinValue;
        MaxX = -double.MaxValue;
        MinY = double.MaxValue;
        MaxY = -double.MaxValue;
    }
    public CPPOGREnvelope(double _minx, double _miny, double _maxx, double _maxy)
    {
        MinX = _minx;
        MaxX = _maxx;
        MinY = _miny;
        MaxY = _maxy;
    }
    public bool IsInit()
    {
        return MinX != double.MaxValue;
    }

    public void Merge(CPPOGREnvelope sOther)
    {
        MinX = Math.Min(MinX, sOther.MinX);
        MaxX = Math.Max(MaxX, sOther.MaxX);
        MinY = Math.Min(MinY, sOther.MinY);
        MaxY = Math.Max(MaxY, sOther.MaxY);
    }

    public void Merge(double dfX, double dfY)
    {
        MinX = Math.Min(MinX, dfX);
        MaxX = Math.Max(MaxX, dfX);
        MinY = Math.Min(MinY, dfY);
        MaxY = Math.Max(MaxY, dfY);
    }

    public void Intersect(CPPOGREnvelope sOther)
    {
        if (Intersects(sOther))
        {
            if (IsInit())
            {
                MinX = Math.Max(MinX, sOther.MinX);
                MaxX = Math.Min(MaxX, sOther.MaxX);
                MinY = Math.Max(MinY, sOther.MinY);
                MaxY = Math.Min(MaxY, sOther.MaxY);
            }
            else
            {
                MinX = sOther.MinX;
                MaxX = sOther.MaxX;
                MinY = sOther.MinY;
                MaxY = sOther.MaxY;
            }
        }
        else
        {
            MinX = double.MinValue;
            MaxX = -double.MaxValue;
            MinY = double.MaxValue;
            MaxY = -double.MaxValue;
        }
    }
    public bool Intersects(CPPOGREnvelope other)
    {
        return MinX <= other.MaxX && MaxX >= other.MinX &&
               MinY <= other.MaxY && MaxY >= other.MinY;
    }
    public bool Contains(CPPOGREnvelope other)
    {
        return MinX <= other.MinX && MinY <= other.MinY &&
               MaxX >= other.MaxX && MaxY >= other.MaxY;
    }

    public Vector2D GetCenter()
    {
        return new Vector2D((MinX + MaxX) / 2, (MinY + MaxY) / 2);
    }

    public double GetHeight()
    {
        return Math.Abs(MaxY - MinY);
    }

    public double GetWidth()
    {
        return Math.Abs(MaxX - MinX);
    }

    public void Translate(Vector2D offset)
    {
        MinX += offset.x;
        MaxX += offset.x;
        MinY += offset.y;
        MaxY += offset.y;
    }

    public Vector2D GetSize()
    {
        return new Vector2D(GetWidth(), GetHeight());
    }

    public override string ToString()
    {
        return string.Format("minx:{0:N4}, maxx:{1:N4}, miny:{2:N4}, maxy:{3:N4}", MinX, MaxX, MinY, MaxY);
    }

    public void ViewToMap(GisViewer v)
    {
        var min = v.ViewToMap(MinX, MinY);
        var max = v.ViewToMap(MaxX, MaxY);
        MinX = min.x;
        MinY = min.y;
        MaxX = max.x;
        MaxY = max.y;
    }
}