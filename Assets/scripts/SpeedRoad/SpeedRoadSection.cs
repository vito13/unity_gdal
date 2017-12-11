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
                    SetTextureWait(vertexWaitingHead, waitingHead, true, e2s_ways);
                    GameObject waitingTail = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadWaitingLength, "waitingtail", ref vertexWaitingTail, false);
                    SetTextureWait(vertexWaitingTail, waitingTail, false, s2e_ways);
                }
            }
            else // 单行道
            {
                if (roadCenterlen - SpeedRoad.RoadWaitingLength > 0) // 分割单向候车区
                {
                    if (e2s_ways > 0)
                    {
                        GameObject waitingHead = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadWaitingLength, "waitinghead", ref vertexWaitingHead);
                        SetTextureWait(vertexWaitingHead, waitingHead, true, e2s_ways, true);
                    }
                    else if (s2e_ways > 0)
                    {
                        GameObject waitingTail = SpeedRoadUtils.CreateMesh_Part(ref vertexbuf, feaObj, SpeedRoad.RoadWaitingLength, "waitingtail", ref vertexWaitingTail, false);
                        SetTextureWait(vertexWaitingTail, waitingTail, false, s2e_ways, true);
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

    void SetTextureWait(List<Vector3> vertex, GameObject part, bool bhead, int ways, bool oneway = false)
    {
        List<float> lstTopPartLen = new List<float>();
        List<float> lstBottomPartLen = new List<float>();
        var len = SpeedRoadUtils.GetRoad2SideLen(vertex, ref lstTopPartLen, ref lstBottomPartLen);
        Vector2[] uv1 = new Vector2[vertex.Count];

        if (bhead)
        {
            uv1[0].Set(0, 1);
            uv1[1].Set(0, 0);
            Vector2 offset = Vector2.zero;
            for (int i = 2; i <= vertex.Count - 2; i += 2)
            {
                var ratetop = (offset.x + lstTopPartLen[i / 2 - 1]) / len.x;
                uv1[i].Set(ratetop, 1);
                var ratebottom = (offset.y + lstBottomPartLen[i / 2 - 1]) / len.y;
                uv1[i + 1].Set(ratebottom, 0);
                offset.x += lstTopPartLen[i / 2 - 1];
                offset.y += lstBottomPartLen[i / 2 - 1];
            }
            uv1[vertex.Count - 2].Set(1, 1);
            uv1[vertex.Count - 1].Set(1, 0);
        }
        else
        {
            uv1[0].Set(1, 0);
            uv1[1].Set(1, 1);
            Vector2 offset = Vector2.zero;
            for (int i = 2; i <= vertex.Count - 2; i += 2)
            {
                var ratetop = (offset.x + lstTopPartLen[i / 2 - 1]) / len.x;
                uv1[i].Set(1 - ratetop, 0);
                var ratebottom = (offset.y + lstBottomPartLen[i / 2 - 1]) / len.y;
                uv1[i + 1].Set(1 - ratebottom, 1);
                offset.x += lstTopPartLen[i / 2 - 1];
                offset.y += lstBottomPartLen[i / 2 - 1];
            }
            uv1[vertex.Count - 2].Set(0, 0);
            uv1[vertex.Count - 1].Set(0, 1);
        }
        part.GetComponent<MeshFilter>().mesh.uv = uv1;

        if (oneway)
        {
            Texture2D tex = SpeedRoadTexMgr.Instance.GetTex("wait" + ways.ToString() + "ways");

//             Color32[] colmain = tex.GetPixels32();
//             var te = new Texture2D(tex.width, tex.height);
//             te.SetPixels32(colmain);
// 
// 
//             Texture2D arrowtex = SpeedRoadTexMgr.Instance.GetTex("arrowforward");
//             Color32[] colarrow = arrowtex.GetPixels32();
//             te.SetPixels32(colarrow);
//             for (int m = 0; m < 128; m++)
//             {
//                 for (int n = 0; n < 128; n++)
//                 {
//                     te.SetPixel(m, n, Color.red);
//                 }
//             }
//             te.Apply();
            part.GetComponent<MeshRenderer>().material.mainTexture = tex;
            part.GetComponent<MeshRenderer>().material.shader = Shader.Find("Transparent/Diffuse");
        }
        else
        {
            Material mat = part.GetComponent<MeshRenderer>().material;
            var shader = Shader.Find("speedroad/waitarea");
            mat.shader = shader;
            var textop = SpeedRoadTexMgr.Instance.GetTex("wait" + ways.ToString() + "waystop");
            Texture2D texbottom = SpeedRoadTexMgr.Instance.GetTex("wait" + ways.ToString() + "waysbottom");
            mat.SetTexture("_Tex1", textop);
            mat.SetTexture("_Tex2", texbottom);
            var repeatU = (len.x + len.y) / 2 / SpeedRoad.RoadwayWidth;
            mat.SetTextureScale("_Tex2", new Vector2(repeatU, 1));
        }
    }

    void SetTexture(List<Vector3> vertex, GameObject part, string texname, float repeatV)
    {
        var tex = SpeedRoadTexMgr.Instance.GetTex(texname);
        part.GetComponent<MeshRenderer>().material.mainTexture = tex;
        part.GetComponent<MeshRenderer>().material.shader = Shader.Find("Transparent/Diffuse");


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

    public HashSet<long> GetCrossings()
    {
        HashSet<long> result = new HashSet<long>();
        if (StartCorssing > -1 && s2e_ways > 0)
        {
            result.Add(StartCorssing);
        }
        if (EndCrossing > -1 && s2e_ways > 0)
        {
            result.Add(EndCrossing);
        }
        return result;
    }

    public List<Vector3> GetPath(bool forward)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < vertexZebraCrossingHead.Count; i += 2)
        {
            result.Add(SpeedRoadUtils.SwapYZ(vertexZebraCrossingHead[i]));
        }
        for (int i = 0; i < vertexWaitingHead.Count; i += 2)
        {
            result.Add(SpeedRoadUtils.SwapYZ(vertexWaitingHead[i]));
        }
        for (int i = 0; i < vertexRoad.Count; i += 2)
        {
            result.Add(SpeedRoadUtils.SwapYZ(vertexRoad[i]));
        }
        for (int i = 0; i < vertexWaitingTail.Count; i += 2)
        {
            result.Add(SpeedRoadUtils.SwapYZ(vertexWaitingTail[i]));
        }
        for (int i = 0; i < vertexZebraCrossingTail.Count; i += 2)
        {
            result.Add(SpeedRoadUtils.SwapYZ(vertexZebraCrossingTail[i]));
        }

        if (!forward)
        {
            result.Reverse();
        }
        return result;
    }
}
