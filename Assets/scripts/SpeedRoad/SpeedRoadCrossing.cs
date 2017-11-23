using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;
using UnityEngine.Assertions;


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
    List<int> lstSections = new List<int>();
    public List<int> Sections
    {
        get
        {
            return lstSections;
        }
    }


    public SpeedRoadCrossing(ref GameObject obj, Feature fea)
    {
        Assert.IsTrue(fea.GetGeometryRef().GetGeometryType() == wkbGeometryType.wkbPoint);
        Init(ref obj, fea);
    }

    void Init(ref GameObject obj, Feature fea)
    {
        fid = fea.GetFID();
        string sections = fea.GetFieldAsString("sections");
        var arr = sections.Split(',');
        foreach (var item in arr)
        {
            lstSections.Add(int.Parse(item));
        }
        Geometry geo = fea.GetGeometryRef();
        pos.x = (float)geo.GetX(0);
        pos.z = (float)geo.GetY(0);


        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.transform.parent = obj.transform;
        ball.transform.position = pos;

        obj.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        obj.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.5f);
        obj.name = fid.ToString();


        /*
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
        */
    }
}
