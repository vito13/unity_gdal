using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class SpeedRoadCrossing
{
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

    Vector3 pos = Vector3.zero;
    List<SpeedRoadSection> lstSections = new List<SpeedRoadSection>();
    

    public SpeedRoadCrossing(ref GameObject obj, Feature fea, SpeedRoadSectionMgr secmgr)
    {
        Assert.IsTrue(fea.GetGeometryRef().GetGeometryType() == wkbGeometryType.wkbPoint);
        Init(ref obj, fea, secmgr);
    }


    List<Vector3> ObtainRingPoints(string secs, SpeedRoadSectionMgr secmgr)
    {
        var arr = secs.Split(',');
        foreach (var item in arr)
        {
            lstSections.Add(secmgr.GetSection(int.Parse(item)));
        }
        
        return SortSections(lstSections);
    }

    void Init(ref GameObject obj, Feature fea, SpeedRoadSectionMgr secmgr)
    {
        fid = fea.GetFID();

        // 构建顶点数组
        var lstpts = ObtainRingPoints(fea.GetFieldAsString("sections"), secmgr);
        Geometry geo = fea.GetGeometryRef();
        lstpts.Add(new Vector3((float)geo.GetX(0), 0, (float)geo.GetY(0)));

        // 构建索引数组
        // 3 * (lstpts.Count);
        List<int> idx = new List<int>();
        for (int i = 0; i < lstpts.Count - 2; i++)
        {
            idx.Add(lstpts.Count - 1);
            idx.Add(i);
            idx.Add(i + 1);
        }
        idx.Add(lstpts.Count - 1);
        idx.Add(lstpts.Count - 2);
        idx.Add(0);

        Mesh msh = new Mesh();
        msh.vertices = lstpts.ToArray();
        msh.triangles = idx.ToArray();
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        obj.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        obj.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.5f);
        obj.name = fid.ToString();
    }

    List<Vector3> SortSections(List<SpeedRoadSection> lst)
    {
        Dictionary<float, List<Vector3>> map = new Dictionary<float, List<Vector3>>();
        foreach (var sec in lst)
        {
            float angle = 0;
            if (sec.StartCorssing == Fid)
            {
                /*var dst = sec.PtArr[2] - sec.PtArr[0];
                angle = SpeedRoadUtils.AngleBetween(Vector3.right, new Vector3(dst.x, dst.z, 0));*/
                List<Vector3> value = new List<Vector3>();
                angle = sec.GetRoadAngle(true, ref value);
                // value.Add(sec.PtArr[0]);
                // value.Add(sec.PtArr[1]);
                
                map[angle] = value;
                continue;
            }
            if (sec.EndCrossing == Fid)
            {
//                 var dst = sec.PtArr[sec.PtArr.Length - 4] - sec.PtArr[sec.PtArr.Length - 2];
//                 angle = SpeedRoadUtils.AngleBetween(Vector3.right, new Vector3(dst.x, dst.z, 0));
                List<Vector3> value = new List<Vector3>();
                angle = sec.GetRoadAngle(false, ref value);
                //                 value.Add(sec.PtArr[sec.PtArr.Length - 1]);
                //                 value.Add(sec.PtArr[sec.PtArr.Length - 2]);
                map[angle] = value;
                continue;   
            }
        }
        var dicSort = from objDic in map orderby objDic.Key descending select objDic;

        List<Vector3> result = new List<Vector3>();
        foreach (KeyValuePair<float, List<Vector3>> kvp in dicSort)
        {
            result.AddRange(kvp.Value);
        }
        return result;
    }
}
