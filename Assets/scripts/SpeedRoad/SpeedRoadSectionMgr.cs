using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using UnityEngine.Assertions;

public class SpeedRoadSectionMgr
{
    Transform parent = null;
    Dictionary<long, SpeedRoadSection> map = new Dictionary<long, SpeedRoadSection>();


    public void LoadFile(string fname, Transform par, GameObject prefab)
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
            //              if (feat.GetFID() != 62)
            //              {
            //                  continue;
            //              }
            GameObject obj = GameObject.Instantiate(prefab);
            obj.transform.parent = parent;
            SpeedRoadSection sec = new SpeedRoadSection(ref obj, feat);
            map[sec.Fid] = sec;
        }
    }

    public SpeedRoadSection GetSection(long fid)
    {
        SpeedRoadSection result = null;
        if (map.ContainsKey(fid))
        {
            result = map[fid];
        }
        return result;
    }
}
