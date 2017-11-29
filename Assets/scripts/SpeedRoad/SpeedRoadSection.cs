using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

public class SpeedRoadSection
{
    static readonly int magicnum = 4;
    int original = -1;
    long fid = -1;
    public long Fid
    {
        get
        {
            return fid;
        }
        set
        {
            fid = Fid;
        }
    }

    int startCorssing = -1;
    public int StartCorssing
    {
        get
        {
            return startCorssing;
        }
        set
        {
            startCorssing = StartCorssing;
        }
    }
    int endCrossing = -1;
    public int EndCrossing
    {
        get
        {
            return endCrossing;
        }
        set
        {
            endCrossing = EndCrossing;
        }
    }

    int s2e_ways = -1;
    int e2s_ways = -1;
    Vector3[] ptarr = null;
    public Vector3[] PtArr
    {
        get
        {
            return ptarr;
        }
        set
        {
            ptarr = PtArr;
        }
    }

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
    

    void Init(ref GameObject obj, Feature fea)
    {
        fid = fea.GetFID();
        original = fea.GetFieldAsInteger("original");
        startCorssing = fea.GetFieldAsInteger("start");
        endCrossing = fea.GetFieldAsInteger("end");
        s2e_ways = fea.GetFieldAsInteger("s2e_ways");
        e2s_ways = fea.GetFieldAsInteger("e2s_ways");
        roadwidth = (s2e_ways + e2s_ways) * SpeedRoad.RoadwayWidth;

        var pts = SpeedRoadUtils.RemoveSamePoint(fea.GetGeometryRef());
        var lstpts = SpeedRoadUtils.OptimizeLine(pts);
        SpeedRoadUtils.CutCrossing(startCorssing, endCrossing, ref lstpts, roadwidth);

        // uv分段处理
        int lindex = 0; // 存储路段两端人行道uv
        int rindex = 0;
        var lstAddPart = PartHandle(ref lstpts, ref lindex, ref rindex);

        // 构建顶点数组
        ptarr = new Vector3[magicnum * (lstpts.Count - 1)];
        int index = 0;
        for (int i = 0; i < lstpts.Count - 1; i++)
        {
            Widen(lstpts[i], lstpts[i + 1], index);
            bool bisaddpart = lstAddPart.Contains(lstpts[i]);
            if (!bisaddpart)
            {
                Corner(index);
            }
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

        // 
        
       

        Mesh msh = new Mesh();
        msh.vertices = ptarr;
        msh.triangles = idx.ToArray();



//         var te = new Texture2D(
//             (s2e_ways + e2s_ways) * SpeedRoad.RoadZebraCrossings4Way * 2,
//             5,
//             TextureFormat.RGBA32, 
//             false);




        {
//             var width = te.width;
//             var height = te.height;
//             var pixels = te.GetPixels32();
//             int offs = 0;
// 
//             Color32 colroad = Color.gray;
//             Color32 colline = Color.white;
//             Color32 colsolid = Color.yellow;
// 
//             te.SetPixel(0, 0, colline);
//             te.SetPixel(0, 2, colline);
//             te.SetPixel(0, 4, colline);
//             te.SetPixel(2, 0, colsolid);
//             te.SetPixel(2, 4, colsolid);
//             te.SetPixel(4, 0, colline);
//             te.SetPixel(4, 2, colline);
//             te.SetPixel(4, 4, colline);

            //             for (int i = 0; i < height; i += 2)
            //             {
            //                 te.SetPixel(i, 0, colline);
            //                 te.SetPixel(i, 4, colline);
            //             }
            //             for (int m = 0; m < height; m++)
            //             {
            //                 for (int n = 1; n < width - 2; n++)
            //                 {
            //                     te.SetPixel(m, n, colroad);
            //                 }
            //             }
            //             te.SetPixel(0, 2, Color.yellow);
            //             te.SetPixel(height - 1, 2, Color.yellow);
//             te.Apply();

            
            List<Vector2> uvZebraCrossing = new List<Vector2>();
            uvZebraCrossing.Add(new Vector2(0, 1));
            uvZebraCrossing.Add(new Vector2(0, 0));
            float ratio = 0.4f;
            rindex = ptarr.Length / 2 - 1 - rindex;
            bool lindexok = false;
            for (int i = 1; i < ptarr.Length / 2 - 1; i++)
            {
                if (i == lindex)
                {
                    ratio = 0.2f;
                }
                else if (i == rindex)
                {
                    ratio = 0.8f;
                }
                uvZebraCrossing.Add(new Vector2(ratio, 1));
                uvZebraCrossing.Add(new Vector2(ratio, 0));
            }
            uvZebraCrossing.Add(new Vector2(1, 1));
            uvZebraCrossing.Add(new Vector2(1, 0));

            msh.uv = uvZebraCrossing.ToArray();
            
        }
         




        msh.RecalculateNormals();
        msh.RecalculateBounds();

        obj.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        obj.GetComponent<MeshRenderer>().material.color = Color.gray;
        obj.name = fid.ToString();
        obj.GetComponent<MeshRenderer>().material.mainTexture = SpeedRoad.RoadTexture2D;
    //    byte[] bytes = te.EncodeToPNG();
    //    File.WriteAllBytes(Application.dataPath + "/onPcSavedScreen.png", bytes);
    }

    


    

   

    List<Vector3> PartHandle(ref List<Vector3> lst, ref int lindex, ref int rindex)
    {
        List<Vector3> result = new List<Vector3>();
        List<float> lstPartLen = new List<float>();
        var roadlen = SpeedRoadUtils.GetRoadLen(lst, ref lstPartLen);
        float minimumZebraCrossingInSection = SpeedRoad.RoadZebraCrossingLength * 2; // 两端斑马线最少占用长度
        float surplus = roadlen - minimumZebraCrossingInSection;
        if (surplus > 0)
        {
            var head = SpeedRoadUtils.AddZebraCrossingHead(lstPartLen, ref lst, ref lindex);
            result.Add(head);
            SpeedRoadUtils.GetRoadLen(lst, ref lstPartLen);
            var tail = SpeedRoadUtils.AddZebraCrossingTail(lstPartLen, ref lst, ref rindex);
            result.Add(tail);
        }
        return result;
    }

    


   

   
    void Widen(Vector3 start, Vector3 end, int index)
    {
        float rstart = SpeedRoadUtils.AngleBetween(Vector3.right, end - start);
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
            bool r = SpeedRoadUtils.Calculate2LineCrossing(ptarr[index - 4], ptarr[index - 2], ptarr[index], ptarr[index + 2], ref crossing);
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
            bool r = SpeedRoadUtils.Calculate2LineCrossing(ptarr[index - 3], ptarr[index - 1], ptarr[index + 1], ptarr[index + 3], ref crossing);
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
