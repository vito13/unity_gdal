using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;

public class SpeedRoadSection
{
    static public readonly int MAGICNUM = 4;
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

    List<Vector3> vertexZebraCrossingHead = new List<Vector3>();
    List<Vector3> vertexZebraCrossingTail = new List<Vector3>();
    List<Vector3> vertexWaitingHead = new List<Vector3>();
    List<Vector3> vertexWaitingTail = new List<Vector3>();
    List<Vector3> vertexRoad = new List<Vector3>();
    float roadwidth = 0;

    public SpeedRoadSection(ref GameObject feaObj, Feature fea)
    {
        var geo = fea.GetGeometryRef();
        Assert.IsTrue(
            geo.GetGeometryType() == wkbGeometryType.wkbLineString &&
            geo.GetPointCount() >= 2
            );
        Init(ref feaObj, fea);
    }


    void Init(ref GameObject feaObj, Feature fea)
    {
        fid = fea.GetFID();
        original = fea.GetFieldAsInteger("original");
        startCorssing = fea.GetFieldAsInteger("start");
        endCrossing = fea.GetFieldAsInteger("end");
        s2e_ways = fea.GetFieldAsInteger("s2e_ways");
        e2s_ways = fea.GetFieldAsInteger("e2s_ways");
        roadwidth = (s2e_ways + e2s_ways) * SpeedRoad.RoadwayWidth;


        var pts = SpeedRoadUtils.RemoveSamePoint(fea.GetGeometryRef());
        pts = SpeedRoadUtils.OptimizeLine(pts);
        SpeedRoadUtils.CutCrossing(startCorssing, endCrossing, ref pts, roadwidth);
        List<Vector3> vertex = new List<Vector3>();
        for (int i = 0; i < MAGICNUM * (pts.Count - 1); i++)
        {
            vertex.Add(Vector3.zero);
        }

        int ptindex = 0;
        for (int i = 0; i < pts.Count - 1; i++)
        {
            SpeedRoadUtils.Widen(ref vertex, pts[i], pts[i + 1], ptindex, roadwidth);
            SpeedRoadUtils.Corner(ref vertex, ptindex);
            ptindex += MAGICNUM;
        }


        CreateMesh(ref vertex, feaObj);
        feaObj.name = fid.ToString();
    }



    void CreateMesh(ref List<Vector3> vertexbuf, GameObject feaObj)
    {
        List<float> lstTopPartLen = new List<float>();
        List<float> lstBottomPartLen = new List<float>();
        var road2Sidelen = SpeedRoadUtils.GetRoad2SideLen(vertexbuf, ref lstTopPartLen, ref lstBottomPartLen);
        var roadCenterlen = (road2Sidelen.x + road2Sidelen.y) / 2;

        string roadtype = "s" + s2e_ways.ToString() + "e" + e2s_ways.ToString();
        if (roadCenterlen - SpeedRoad.RoadZebraCrossingLength * 2 > 0) // 两端斑马线最少占用长度
        {

            GameObject zebraCrossingHead = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadZebraCrossingLength, "zchead", ref vertexZebraCrossingHead);
            SetTexture(vertexZebraCrossingHead, zebraCrossingHead, "ZebraCrossing", 4);
            GameObject zebraCrossingTail = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadZebraCrossingLength, "zctail", ref vertexZebraCrossingTail, false);
            SetTexture(vertexZebraCrossingTail, zebraCrossingTail, "ZebraCrossing", 4);

            road2Sidelen = SpeedRoadUtils.GetRoad2SideLen(vertexbuf, ref lstTopPartLen, ref lstBottomPartLen);
            roadCenterlen = (road2Sidelen.x + road2Sidelen.y) / 2;
            bool bNotOneWay = e2s_ways > 0 && s2e_ways > 0;
            if (bNotOneWay) // 双向车道
            {
                if (roadCenterlen - SpeedRoad.RoadWaitingLength * 2 > 0) // 分割双向候车区
                {
                    GameObject waitingHead = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadWaitingLength, "waitinghead", ref vertexWaitingHead);
                    SetTexture(vertexWaitingHead, waitingHead, roadtype + "wait", 1);
                    GameObject waitingTail = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadWaitingLength, "waitingtail", ref vertexWaitingTail, false);
                }
            }
            else // 单行道
            {
                if (roadCenterlen - SpeedRoad.RoadWaitingLength > 0) // 分割单向候车区
                {
                    if (e2s_ways > 0)
                    {
                        GameObject waitingHead = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadWaitingLength, "waitinghead", ref vertexWaitingHead);
                    }
                    else if (s2e_ways > 0)
                    {
                        GameObject waitingTail = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadWaitingLength, "waitingtail", ref vertexWaitingTail, false);
                    }
                    else
                    {
                        Assert.IsTrue(false);
                    }
                }
            }
        }
        GameObject road = SpeedRoadUtils.CreateSegment(vertexbuf, feaObj, "road");
        
        SetTexture(vertexbuf, road, roadtype, 1);
        vertexRoad = vertexbuf;
    }

    void SetTexture(List<Vector3> vertex, GameObject part, string texname, float repeatV)
    {
        var tex = SpeedRoadTexMgr.Instance.GetTex(texname);
        part.GetComponent<MeshRenderer>().material.mainTexture = tex;
        List<float> lstTopPartLen = new List<float>();
        List<float> lstBottomPartLen = new List<float>();
        var len = SpeedRoadUtils.GetRoad2SideLen(vertex, ref lstTopPartLen, ref lstBottomPartLen);
        Vector2[] uv1 = new Vector2[vertex.Count];
        float repeatU = 1;
        if (part.name == "road")
        {
            repeatU = (len.x + len.y) / 2 / SpeedRoad.RoadwayWidth;
        }
        uv1[0].Set(0, repeatV);
        uv1[1].Set(0, 0);
        Vector2 offset = Vector2.zero;
        for (int i = 2; i <= vertex.Count - 2; i += 2)
        {
            var ratetop = (offset.x + lstTopPartLen[i / 2 - 1]) / len.x;
            uv1[i].Set(repeatU * ratetop, repeatV);
            var ratebottom = (offset.y + lstBottomPartLen[i / 2 - 1]) / len.y;
            uv1[i + 1].Set(repeatU * ratebottom, 0);
            offset.x += lstTopPartLen[i / 2 - 1];
            offset.y += lstBottomPartLen[i / 2 - 1];
        }
        uv1[vertex.Count - 2].Set(repeatU, repeatV);
        uv1[vertex.Count - 1].Set(repeatU, 0);
        part.GetComponent<MeshFilter>().mesh.uv = uv1;
    }

    public float GetRoadAngle(bool bhead, ref List<Vector3> lst)
    {
        float angle = 0;
        List<Vector3> vertex = null;
        if (bhead)
        {
            if (vertexZebraCrossingHead.Count > 0)
            {
                vertex = vertexZebraCrossingHead;
            }
            else if (vertexRoad.Count > 0)
            {
                vertex = vertexRoad;
            }
            else
            {
                Assert.IsFalse(true);
            }
            var dst = vertex[2] - vertex[0];
            angle = SpeedRoadUtils.AngleBetween(Vector3.right, new Vector3(dst.x, dst.y, 0));
            lst.Add(SpeedRoadUtils.SwapYZ(vertex[0]));
            lst.Add(SpeedRoadUtils.SwapYZ(vertex[1]));
        }
        else
        {
            if (vertexZebraCrossingTail.Count > 0)
            {
                vertex = vertexZebraCrossingTail;
            }
            else if (vertexRoad.Count > 0)
            {
                vertex = vertexRoad;
            }
            else
            {
                Assert.IsFalse(true);
            }
            var dst = vertex[vertex.Count - 4] - vertex[vertex.Count - 2];
            angle = SpeedRoadUtils.AngleBetween(Vector3.right, new Vector3(dst.x, dst.y, 0));
            lst.Add(SpeedRoadUtils.SwapYZ(vertex[vertex.Count - 1]));
            lst.Add(SpeedRoadUtils.SwapYZ(vertex[vertex.Count - 2]));
        }
        return angle;
    }
}
