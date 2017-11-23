using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;
using UnityEngine.Assertions;

public class SpeedRoadSection{
    static readonly int magicnum = 4;
    int original = -1;
    long fid = -1;
    int startCorssing = -1;
    int endCrossing = -1;
    int s2e_ways = -1;
    int e2s_ways = -1;
    Vector3[] ptarr = null;
    float roadwidth = 0;

    public SpeedRoadSection(ref GameObject obj, Feature fea)
    {
        var geo = fea.GetGeometryRef();
        Assert.IsTrue(
            geo.GetGeometryType() == wkbGeometryType.wkbLineString &&
            geo.GetPointCount() >= 2
            );
        Init(ref obj, fea);
    }

    public long GetFid()
    {
        return fid;
    }

    void CreateGeo(List<Vector3> lst, ref Geometry dst)
    {
        foreach (var item in lst)
        {
            dst.AddPoint_2D(item.x, item.y);
        }
        Assert.IsTrue(dst.GetPointCount() >= 2);
    }

    List<Vector3> RemoveSamePoint(Geometry src)
    {
        List<Vector3> result = new List<Vector3>();
        double x1 = src.GetX(0);
        double y1 = src.GetY(0);
        result.Add(new Vector3((float)x1, (float)y1));
        int c = src.GetPointCount();
        for (int i = 1; i < c; i++)
        {
            double x2 = src.GetX(i);
            double y2 = src.GetY(i);
            if (x1 != x2 || y1 != y2)
            {
                result.Add(new Vector3((float)x2, (float)y2));
                x1 = x2;
                y1 = y2;
            }
//             else
//             {
//                 Debug.Log("去除相同点");
//             }
        }
        Assert.IsTrue(result.Count >= 2);
        return result;
    }

    List<Vector3> OptimizeLine(List<Vector3> lst)
    {
        if (lst.Count == 2)
        {
            return lst;
        }
        List<Vector3> result = new List<Vector3>();
        result.Add(lst[0]);
        result.Add(lst[1]);

        var d1 = AngleBetween(Vector3.right, lst[1] - lst[0]);
        for (int i = 2; i < lst.Count; i++)
        {
            var d2 = AngleBetween(Vector3.right, lst[i] - lst[i - 1]);
            if (Mathf.Abs(d1 - d2) < SpeedRoad.AngleThreshold)
            {
                result.RemoveAt(result.Count - 1);
//                 Debug.Log("优化线段");
            }
            result.Add(lst[i]);
            d1 = d2;
        }

        Assert.IsTrue(result.Count >= 2);
        return result;
    }

    void Init(ref GameObject obj, Feature fea)
    {
        fid = fea.GetFID();
        original = fea.GetFieldAsInteger("original");
        startCorssing = fea.GetFieldAsInteger("start");
        endCrossing = fea.GetFieldAsInteger("end");
        s2e_ways = fea.GetFieldAsInteger("s2e_ways");
        e2s_ways = fea.GetFieldAsInteger("e2s_ways");
        roadwidth = (s2e_ways + e2s_ways) * SpeedRoad.RoadwayWidth;

        Geometry ls = new Geometry(wkbGeometryType.wkbLineString);
        {
            var pts = RemoveSamePoint(fea.GetGeometryRef());
            var result = OptimizeLine(pts);
            CutCrossing(ref result, roadwidth);
            CreateGeo(result, ref ls);
        }

        // 取线上所有点
        List<Vector3> lstpts = new List<Vector3>();
        for (int i = 0; i < ls.GetPointCount(); i++)
        {
            lstpts.Add(new Vector3((float)ls.GetX(i), (float)ls.GetY(i), 0));
        }

        // 构建顶点数组
        ptarr = new Vector3[magicnum * (lstpts.Count - 1)];
        int index = 0;
        for (int i = 0; i < lstpts.Count - 1; i++)
        {
            Widen(lstpts[i], lstpts[i + 1], index);
            Corner(index);
            index += magicnum;
        }


        for (int i = 0; i < ptarr.Length; i++)
        {
            ptarr[i].z = ptarr[i].y;
            ptarr[i].y = 0;
        }

        // 构建索引数组
        // 6 * (lstpts.Count * 2 - 3);
        List<int> idx = new List<int>();
        for (int i = 0; i < ptarr.Length - 2; i += 2)
        {
            idx.Add(i + 0);
            idx.Add(i + 2);
            idx.Add(i + 3);
            idx.Add(i + 0);
            idx.Add(i + 3);
            idx.Add(i + 1);
        }

        Mesh msh = new Mesh();
        msh.vertices = ptarr;
        msh.triangles = idx.ToArray();
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        obj.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        obj.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
        obj.name = fid.ToString();
    }

    void CutCrossing(ref List<Vector3> lst, float cutLength)
    {
        if (startCorssing != -1)
        {
            float d = Vector3.Distance(lst[1], lst[0]);
            if (d >= cutLength)
            {
                lst[0] = Vector3.Lerp(lst[1], lst[0], (d - cutLength) / d);
            }
            else
            {
                lst.RemoveAt(0);
                CutCrossing(ref lst, cutLength - d);
            }
        }
        if (endCrossing != -1)
        {
            float d = Vector3.Distance(lst[lst.Count - 1], lst[lst.Count - 2]);
            if (d >= cutLength)
            {
                lst[lst.Count - 1] = Vector3.Lerp(lst[lst.Count - 2], lst[lst.Count - 1], (d - cutLength) / d);
            }
            else
            {
                lst.RemoveAt(lst.Count - 1);
                CutCrossing(ref lst, cutLength - d);
            }
        }
    }


    static float AngleBetween(Vector3 vector1, Vector3 vector2)
    {
        float sin = vector1.x * vector2.y - vector2.x * vector1.y;
        float cos = vector1.x * vector2.x + vector1.y * vector2.y;
        return Mathf.Atan2(sin, cos) * (180 / Mathf.PI);
    }

    static bool Calculate2LineCrossing(Vector3 l1start, Vector3 l1end, Vector3 l2start, Vector3 l2end, ref Vector3 crossing)
    {
        bool r = false;
        Geometry l1 = new Geometry(wkbGeometryType.wkbLineString);
        l1.AddPoint_2D(l1start.x, l1start.y);
        l1.AddPoint_2D(l1end.x, l1end.y);

        Geometry l2 = new Geometry(wkbGeometryType.wkbLineString);
        l2.AddPoint_2D(l2start.x, l2start.y);
        l2.AddPoint_2D(l2end.x, l2end.y);
        Geometry intersection = l1.Intersection(l2);
        intersection.FlattenTo2D();
        wkbGeometryType t = intersection.GetGeometryType();
        if (wkbGeometryType.wkbPoint == t)
        {
            r = true;
            crossing.x = (float)intersection.GetX(0);
            crossing.y = (float)intersection.GetY(0);
        }
        return r;
    }

    void Widen(Vector3 start, Vector3 end, int index)
    {
        float rstart = AngleBetween(Vector3.right, end - start);
        Quaternion rotation = Quaternion.Euler(0, 0, -rstart);
        Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        Vector3 ss = m.MultiplyPoint(start);
        Vector3 se = m.MultiplyPoint(end);
        Matrix4x4 m2 = m.inverse;
        
        Vector3 halfWidth = new Vector3(0, roadwidth / 2, 0);
        ptarr[index + 0] = m2.MultiplyPoint(ss + halfWidth);
        ptarr[index + 1] = m2.MultiplyPoint(ss - halfWidth);
        ptarr[index + 2] = m2.MultiplyPoint(se + halfWidth);
        ptarr[index + 3] = m2.MultiplyPoint(se - halfWidth);
    }

    void Corner(int index)
    {
        if (index == 0)
        {
            return;
        }
        Assert.IsTrue(index % magicnum == 0);

        Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
        pt.AddPoint_2D(ptarr[index].x, ptarr[index].y);
        Geometry dst = new Geometry(wkbGeometryType.wkbPolygon);
        Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
        ring.AddPoint_2D(ptarr[index - 4].x, ptarr[index - 4].y);
        ring.AddPoint_2D(ptarr[index - 2].x, ptarr[index - 2].y);
        ring.AddPoint_2D(ptarr[index - 1].x, ptarr[index - 1].y);
        ring.AddPoint_2D(ptarr[index - 3].x, ptarr[index - 3].y);
        ring.CloseRings();
        dst.AddGeometry(ring);
        dst.FlattenTo2D();

        Vector3 crossing = new Vector3();
        if (pt.Within(dst))
        {
            bool r = Calculate2LineCrossing(ptarr[index - 4], ptarr[index - 2], ptarr[index], ptarr[index + 2], ref crossing);
            if (r)
            {
                ptarr[index] = crossing;
                ptarr[index - 2] = crossing;
            }
            else
            {
                Assert.IsFalse(true);
                /*
                var lst = new List<Geometry>();
                lst.Add(dst);
                ShpUtils.SaveShp("c:\\test_\\test_poly.shp", "test_poly", wkbGeometryType.wkbPolygon, lst);
                lst.Clear();
                lst.Add(pt);
                ShpUtils.SaveShp("c:\\test_\\test_p.shp", "test_pt", wkbGeometryType.wkbPoint, lst);
                lst.Clear();

                Geometry l1 = new Geometry(wkbGeometryType.wkbLineString);
                l1.AddPoint_2D(ptarr[index - 4].x, ptarr[index - 4].y);
                l1.AddPoint_2D(ptarr[index - 2].x, ptarr[index - 2].y);

                Geometry l2 = new Geometry(wkbGeometryType.wkbLineString);
                l2.AddPoint_2D(ptarr[index].x, ptarr[index].y);
                l2.AddPoint_2D(ptarr[index + 2].x, ptarr[index + 2].y);
                lst.Add(l1);
                lst.Add(l2);

                var d1 = AngleBetween(Vector3.right, ptarr[index - 2] - ptarr[index - 4]);
                var d2 = AngleBetween(Vector3.right, ptarr[index + 2] -  ptarr[index]);

                ShpUtils.SaveShp("c:\\test_\\test_line.shp", "test_line", wkbGeometryType.wkbLineString, lst);
                */
            }
        }
        else
        {
            bool r = Calculate2LineCrossing(ptarr[index - 3], ptarr[index - 1], ptarr[index + 1], ptarr[index + 3], ref crossing);
            if (r)
            {
                ptarr[index - 1] = crossing;
                ptarr[index + 1] = crossing;
            }
            else
            {
                Assert.IsFalse(true);
            }
        }
    }
}
