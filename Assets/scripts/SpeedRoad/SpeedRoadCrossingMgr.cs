using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using UnityEngine.Assertions;

public class SpeedRoadCrossingMgr : MonoBehaviour {
    Transform parent = null;
    Dictionary<long, SpeedRoadCrossing> map = new Dictionary<long, SpeedRoadCrossing>();

    public void LoadFile(string fname, Transform par, SpeedRoadSectionMgr secmgr)
    {
        parent = par;
        DataSource ds = Ogr.Open(fname, 0);
        Assert.IsNotNull(ds);
        Assert.IsTrue(ds.GetLayerCount() > 0);
        Layer layer = ds.GetLayerByIndex(0);
        layer.ResetReading();
        Feature feat;
        while ((feat = layer.GetNextFeature()) != null)
        {
            GameObject obj = Instantiate(SpeedRoad.prefab);
            obj.transform.parent = parent;
            SpeedRoadCrossing crossing = new SpeedRoadCrossing(ref obj, feat, secmgr);
            map[crossing.Fid] = crossing;
        }
    }

    public SpeedRoadCrossing GetCrossing(long fid)
    {
        if (map.ContainsKey(fid))
        {
            return map[fid];
        }
        return null;
    }

    public Dictionary<long, HashSet<long>> BuildGraph(SpeedRoadSectionMgr secmgr)
    {
        Dictionary<long, HashSet<long>> graph = new Dictionary<long, HashSet<long>>();
        foreach (var item in map)
        {
            graph[item.Key] = item.Value.GetNeighbors(secmgr);
        }
        return graph;
    }


    public List<Vector3> GetSectionFrom2Corssing(long start, long end)
    {
        SpeedRoadCrossing s = GetCrossing(start);
        bool forward = true;
        SpeedRoadSection sec = s.GetTargetSection(end, ref forward);
        Assert.IsNotNull(sec);
        var path = sec.GetPath(forward);
        return path;
    }
}
