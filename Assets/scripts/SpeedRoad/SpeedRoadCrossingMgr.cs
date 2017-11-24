using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using UnityEngine.Assertions;

public class SpeedRoadCrossingMgr : MonoBehaviour {
    Transform parent = null;
    Dictionary<long, SpeedRoadCrossing> map = new Dictionary<long, SpeedRoadCrossing>();

    public void LoadFile(string fname, Transform par, GameObject prefab, SpeedRoadSectionMgr secmgr)
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
            GameObject obj = GameObject.Instantiate(prefab);
            obj.transform.parent = parent;
            SpeedRoadCrossing crossing = new SpeedRoadCrossing(ref obj, feat, secmgr);
            map[crossing.Fid] = crossing;
        }
    }

    public SpeedRoadCrossing GetCrossing(int fid)
    {
        return null;
    }
//     public void CalculateCrossingMesh(SpeedRoadSectionMgr secmgr)
//     {
//         foreach (var item in map)
//         {
//             var crossing = item.Value;
//             var secs = crossing.Sections;
// 
//             List<SpeedRoadSection> lst = new List<SpeedRoadSection>();
//             foreach (var fid in secs)
//             {
//                 var section = secmgr.GetSection(fid);
//                 lst.Add(section);
//             }
// 
//             var sortedlst = crossing.SortSections(lst);
// 
//          }
//     }
}
