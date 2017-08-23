using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;

public class GisModel{
    DataSource ds = null;
    Layer layer = null;

    public void Open(string fname)
    {
        ds = Ogr.Open(fname, 0);
        System.Diagnostics.Debug.Assert(ds != null);
        System.Diagnostics.Debug.Assert(ds.GetLayerCount() > 0);
        layer = ds.GetLayerByIndex(0);
        System.Diagnostics.Debug.Assert(layer != null);
        ResetFeatureIterator();
    }

    public void ResetFeatureIterator()
    {
        layer.ResetReading();
    }

    public string GetNextFeatureGeometryJson()
    {
        string result = "";
        Feature feat;
        while ((feat = layer.GetNextFeature()) != null)
        {
            Geometry geom = feat.GetGeometryRef();
            if (geom != null)
            {
                wkbGeometryType t = Ogr.GT_Flatten(geom.GetGeometryType());
                switch (t)
                {
                    case wkbGeometryType.wkbUnknown:
                        break;
                    case wkbGeometryType.wkbPoint:
                        break;
                    case wkbGeometryType.wkbLineString:
                        {
                            string json = geom.ExportToJson(null);
                            System.Diagnostics.Debug.Assert(json.Length > 0);
                            result = json;
                        }
                        break;
                    default:
                        break;
                }
                
            }
            feat.Dispose();
            if (result.Length > 0)
            {
                break;
            }
        }
        return result;
    }

    public void GetEnvelpoe(ref Rect rc)
    {
        Envelope env = new Envelope();
        int r = layer.GetExtent(env, 1);
        rc.xMin = (float)env.MinX;
        rc.yMin = (float)env.MinY;
        rc.xMax = (float)env.MaxX;
        rc.yMax = (float)env.MaxY;

//         List<float> result = new List<float>();
//         result.Add((float)env.MinX);
//         result.Add((float)env.MinY);
//         result.Add((float)env.MaxX);
//         result.Add((float)env.MaxY);
        env.Dispose();
    }
}
