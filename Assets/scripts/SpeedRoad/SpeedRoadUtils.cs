﻿using OSGeo.OGR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SpeedRoadUtils {
    static public bool SaveShp(string path, string fanme, wkbGeometryType t, List<Geometry> lstgeo)
    {
        // 为了支持中文路径，请添加下面这句代码  
        OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
        // 为了使属性表字段支持中文，请添加下面这句  
        OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");

        string strVectorFile = path;

        // 注册所有的驱动  
        Ogr.RegisterAll();

        //创建数据，这里以创建ESRI的shp文件为例  
        string strDriverName = "ESRI Shapefile";
        Driver oDriver = Ogr.GetDriverByName(strDriverName);
        if (oDriver == null)
        {
            // Console.WriteLine("%s 驱动不可用！\n", strVectorFile);
            return false;
        }

        // 创建数据源  
        DataSource oDS = oDriver.CreateDataSource(strVectorFile, null);
        if (oDS == null)
        {
            // Console.WriteLine("创建矢量文件【%s】失败！\n", strVectorFile);
            return false;
        }

        // 创建图层，创建一个多边形图层，这里没有指定空间参考，如果需要的话，需要在这里进行指定  
        Layer oLayer = oDS.CreateLayer(fanme, null, t, null);
        if (oLayer == null)
        {
            // Console.WriteLine("图层创建失败！\n");
            return false;
        }

        FeatureDefn oDefn = oLayer.GetLayerDefn();

        // 创建三角形要素  
        foreach (var item in lstgeo)
        {
            Feature oFeatureTriangle = new Feature(oDefn);
            oFeatureTriangle.SetGeometry(item);
            oLayer.CreateFeature(oFeatureTriangle);
        }
        
        return true;
    }


    static public List<Vector3> RemoveSamePoint(Geometry src)
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
        }
        Assert.IsTrue(result.Count >= 2);
        return result;
    }

    static public float AngleBetween(Vector3 vector1, Vector3 vector2)
    {
        float sin = vector1.x * vector2.y - vector2.x * vector1.y;
        float cos = vector1.x * vector2.x + vector1.y * vector2.y;
        return Mathf.Atan2(sin, cos) * (180 / Mathf.PI);
    }

    static public List<Vector3> OptimizeLine(List<Vector3> lst)
    {
        if (lst.Count == 2)
        {
            return lst;
        }
        List<Vector3> result = new List<Vector3>();
        result.Add(lst[0]);
        result.Add(lst[1]);

        var d1 = SpeedRoadUtils.AngleBetween(Vector3.right, lst[1] - lst[0]);
        for (int i = 2; i < lst.Count; i++)
        {
            var d2 = SpeedRoadUtils.AngleBetween(Vector3.right, lst[i] - lst[i - 1]);
            if (Mathf.Abs(d1 - d2) < SpeedRoad.RoadAngleThreshold)
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

    static public bool Calculate2LineCrossing(Vector3 l1start, Vector3 l1end, Vector3 l2start, Vector3 l2end, ref Vector3 crossing)
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

    static public Vector3 AddZebraCrossingTail(List<float> lstPartLen, ref List<Vector3> lst, ref int rindex)
    {
        rindex = 1;
        Vector3 tail = Vector3.zero;
        bool bfind = false;
        float dis = SpeedRoad.RoadZebraCrossingLength;
        for (int i = lstPartLen.Count - 1; i >= 0; i--)
        {
            if (lstPartLen[i] >= dis)
            {
                tail = Vector3.Lerp(lst[i], lst[i + 1], (lstPartLen[i] - dis) / lstPartLen[i]);
                lst.Insert(i + 1, tail);
                bfind = true;
                break;
            }
            else
            {
                dis -= lstPartLen[i];
                rindex++;
            }
        }
        Assert.IsTrue(bfind);
        return tail;
    }
    static public List<Vector3> SegmentationPartFromTail(float distance, List<float> lstTopPartLen, List<float> lstBottomPartLen, ref List<Vector3> vertexbuf)
    {
        Assert.IsTrue(lstTopPartLen.Count == lstBottomPartLen.Count);
        Assert.IsTrue(vertexbuf.Count >= 4);
        List<Vector3> vertex = new List<Vector3>();
        {
            int bdel = 0;
            for (int i = lstTopPartLen.Count; i >= 0 ; i--)
            {
                vertex.Add(vertexbuf[i * 2 + 1]);
                vertex.Add(vertexbuf[i * 2]);
                if (lstTopPartLen[i - 1] >= distance)
                {
                    var ptbottom = Vector3.Lerp(vertexbuf[i * 2 + 1], vertexbuf[i * 2 - 1], distance / lstBottomPartLen[i - 1]);
                    vertex.Add(ptbottom);
                    vertexbuf[i * 2 + 1] = ptbottom;
                    var pttop = Vector3.Lerp(vertexbuf[i * 2], vertexbuf[i * 2 - 2], distance / lstTopPartLen[i - 1]);
                    vertex.Add(pttop);
                    vertexbuf[i * 2] = pttop;
                    break;
                }
                else
                {
                    distance -= lstTopPartLen[i - 1];
                    bdel += 2;
                }
            }

            for (int i = 0; i < bdel; i++)
            {
                vertexbuf.RemoveAt(vertexbuf.Count - 1);
            }
        }
        vertex.Reverse();
        Assert.IsTrue(vertex.Count >= 4);
        return vertex;
    }
    static public List<Vector3> SegmentationPartFromHead(float distance, List<float> lstTopPartLen, List<float> lstBottomPartLen, ref List<Vector3> vertexbuf)
    {
        Assert.IsTrue(lstTopPartLen.Count == lstBottomPartLen.Count);
        Assert.IsTrue(vertexbuf.Count >= 4);
        List<Vector3> vertex = new List<Vector3>();
        {
            int bdel = 0;
            for (int i = 0; i < lstTopPartLen.Count; i++)
            {
                vertex.Add(vertexbuf[i * 2]);
                vertex.Add(vertexbuf[i * 2 + 1]);
                if (lstTopPartLen[i] >= distance)
                {
                    var pttop = Vector3.Lerp(vertexbuf[i * 2 + 2], vertexbuf[i * 2], (lstTopPartLen[i] - distance) / lstTopPartLen[i]);
                    vertex.Add(pttop);
                    vertexbuf[i * 2] = pttop;
                    var ptbottom = Vector3.Lerp(vertexbuf[i * 2 + 3], vertexbuf[i * 2 + 1], (lstBottomPartLen[i] - distance) / lstBottomPartLen[i]);
                    vertex.Add(ptbottom);
                    vertexbuf[i * 2 + 1] = ptbottom;
                    break;
                }
                else
                {
                    distance -= lstTopPartLen[i];
                    bdel += 2;
                }
            }
            vertexbuf.RemoveRange(0, bdel);
        }
        Assert.IsTrue(vertex.Count >= 4);
        return vertex;
    }

   

    static public Vector2 GetRoad2SideLen(List<Vector3> lst, ref List<float> lstTopPartLen, ref List<float> lstBottomPartLen)
    {
        lstTopPartLen.Clear();
        lstBottomPartLen.Clear();
        Vector2 result = Vector2.zero;
        for (int i = 0; i < lst.Count - 2; i+=2)
        {
            var lentop = Vector3.Distance(lst[i], lst[i + 2]);
            lstTopPartLen.Add(lentop);
            result.x += lentop;
            var lenbottom = Vector3.Distance(lst[i + 1], lst[i + 3]);
            lstBottomPartLen.Add(lenbottom);
            result.y += lenbottom;
        }
        return result;
    }

    static public void CutCrossing(int startCorssing, int endCrossing, ref List<Vector3> lst, float cutLength)
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
                CutCrossing(startCorssing, endCrossing, ref lst, cutLength - d);
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
                CutCrossing(startCorssing, endCrossing, ref lst, cutLength - d);
            }
        }
    }

    //     static public void CreateGeo(List<Vector3> lst, ref Geometry dst)
    //     {
    //         foreach (var item in lst)
    //         {
    //             dst.AddPoint_2D(item.x, item.y);
    //         }
    //         Assert.IsTrue(dst.GetPointCount() >= 2);
    //     }

    static public void Widen(ref List<Vector3> vertexbuf, Vector3 start, Vector3 end, int index, float roadwidth)
    {
        float rstart = SpeedRoadUtils.AngleBetween(Vector3.right, end - start);
        Quaternion rotation = Quaternion.Euler(0, 0, -rstart);
        Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
        Vector3 ss = m.MultiplyPoint(start);
        Vector3 se = m.MultiplyPoint(end);
        Matrix4x4 m2 = m.inverse;

        Vector3 halfWidth = new Vector3(0, roadwidth / 2, 0);
        vertexbuf[index + 0] = m2.MultiplyPoint(ss + halfWidth);
        vertexbuf[index + 1] = m2.MultiplyPoint(ss - halfWidth);
        vertexbuf[index + 2] = m2.MultiplyPoint(se + halfWidth);
        vertexbuf[index + 3] = m2.MultiplyPoint(se - halfWidth);
    }

    static public void Corner(ref List<Vector3> vertexbuf, int index)
    {
        if (index == 0)
        {
            return;
        }
        Assert.IsTrue(index % SpeedRoadSection.MAGICNUM == 0);

        Geometry pt = new Geometry(wkbGeometryType.wkbPoint);
        pt.AddPoint_2D(vertexbuf[index].x, vertexbuf[index].y);
        Geometry dst = new Geometry(wkbGeometryType.wkbPolygon);
        Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
        ring.AddPoint_2D(vertexbuf[index - 4].x, vertexbuf[index - 4].y);
        ring.AddPoint_2D(vertexbuf[index - 2].x, vertexbuf[index - 2].y);
        ring.AddPoint_2D(vertexbuf[index - 1].x, vertexbuf[index - 1].y);
        ring.AddPoint_2D(vertexbuf[index - 3].x, vertexbuf[index - 3].y);
        ring.CloseRings();
        dst.AddGeometry(ring);
        dst.FlattenTo2D();

        Vector3 crossing = new Vector3();
        if (pt.Within(dst))
        {
            bool r = Calculate2LineCrossing(vertexbuf[index - 4], vertexbuf[index - 2], vertexbuf[index], vertexbuf[index + 2], ref crossing);
            if (r)
            {
                vertexbuf[index] = crossing;
                vertexbuf[index - 2] = crossing;
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
            bool r = Calculate2LineCrossing(vertexbuf[index - 3], vertexbuf[index - 1], vertexbuf[index + 1], vertexbuf[index + 3], ref crossing);
            if (r)
            {
                vertexbuf[index - 1] = crossing;
                vertexbuf[index + 1] = crossing;
            }
            else
            {
                Assert.IsFalse(true);
            }
        }
    }

    static public Vector3 SwapYZ(Vector3 v)
    {
        return new Vector3(v.x, v.z, v.y);
    }

    static public GameObject CreateSegment(List<Vector3> vertex, GameObject feaObj, string name)
    {
        var vertexarr = vertex.ToArray();
        // 构建顶点数组
        for (int i = 0; i < vertexarr.Length; i++)
        {
            vertexarr[i].z = vertexarr[i].y;
            vertexarr[i].y = 0;
        }

        // 构建索引数组
        // 6 * (lstpts.Count * 2 - 3);
        List<int> idx = new List<int>();
        for (int i = 0; i < vertexarr.Length - 2; i += 2)
        {
            idx.Add(i + 0);
            idx.Add(i + 2);
            idx.Add(i + 3);
            idx.Add(i + 0);
            idx.Add(i + 3);
            idx.Add(i + 1);
        }
        List<float> lstTopPartLen = new List<float>();
        List<float> lstBottomPartLen = new List<float>();
        Mesh msh = new Mesh();
        msh.vertices = vertexarr;
        msh.triangles = idx.ToArray();
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        GameObject obj = GameObject.Instantiate(SpeedRoad.prefab);
        obj.transform.parent = feaObj.transform;

        obj.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        obj.name = name;
        return obj;
    }

    static public GameObject CreateMesh_Part(ref List<Vector3> vertexbuf, GameObject feaObj, float length, string name, ref List<Vector3> partVertex, bool bhead = true)
    {
        List<float> lstTopPartLen = new List<float>();
        List<float> lstBottomPartLen = new List<float>();
        GetRoad2SideLen(vertexbuf, ref lstTopPartLen, ref lstBottomPartLen);
        if (bhead)
        {
            partVertex = SegmentationPartFromHead(length, lstTopPartLen, lstBottomPartLen, ref vertexbuf);
        }
        else
        {
            partVertex = SegmentationPartFromTail(length, lstTopPartLen, lstBottomPartLen, ref vertexbuf);
        }
        return CreateSegment(partVertex, feaObj, name);
       
    }
}
