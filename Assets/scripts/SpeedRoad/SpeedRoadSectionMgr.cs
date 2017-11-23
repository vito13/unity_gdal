using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using UnityEngine.Assertions;

public class SpeedRoadSectionMgr {
    Transform parent = null;

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
        }
    }

    public SpeedRoadSection GetSection(int fid)
    {
         Transform t = parent.FindChild(fid.ToString());
         Assert.IsNotNull(t);
         return t.gameObject.GetComponent<SpeedRoadSection>();
    }
}
