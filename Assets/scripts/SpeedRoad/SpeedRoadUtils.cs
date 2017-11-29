using OSGeo.OGR;
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

    static public Vector3 AddZebraCrossingHead(List<float> lstPartLen, ref List<Vector3> lst, ref int lindex)
    {
        lindex = 1;
        Vector3 head = Vector3.zero;
        bool bfind = false;
        float dis = SpeedRoad.RoadZebraCrossingLength;
        for (int i = 0; i < lstPartLen.Count; i++)
        {
            if (lstPartLen[i] >= dis)
            {
                head = Vector3.Lerp(lst[i + 1], lst[i], (lstPartLen[i] - dis) / lstPartLen[i]);
                lst.Insert(i + 1, head);
                bfind = true;
                break;
            }
            else
            {
                dis -= lstPartLen[i];
                lindex++;
            }
        }
        Assert.IsTrue(bfind);
        return head;
    }

    static public float GetRoadLen(List<Vector3> lst, ref List<float> lstPartLen)
    {
        lstPartLen.Clear();
        float result = 0;
        for (int i = 0; i < lst.Count - 1; i++)
        {
            var len = Vector3.Distance(lst[i], lst[i + 1]);
            lstPartLen.Add(len);
            result += len;
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

}
