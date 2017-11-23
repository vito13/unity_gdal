using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;
using UnityEngine.Assertions;

public class GisModel{
    DataSource ds = null;
    Layer layer = null;
    GisSpatialQuery spatialquery = null;
    Enyim.Collections.Envelope mapenv = null; // 所有geo合并的env
    Enyim.Collections.Envelope currentenv = null; // 当前view呈现的map范围


    public void Open(string fname)
    {
        Clear();
        ds = Ogr.Open(fname, 1);
        Assert.IsNotNull(ds);
        Assert.IsTrue(ds.GetLayerCount() > 0);
        layer = ds.GetLayerByIndex(0);
        Assert.IsNotNull(layer);
        

        spatialquery = new GisSpatialQuery();
        spatialquery.Clear();
        ReadFeature2Tree();
        currentenv = new Enyim.Collections.Envelope();
    }

    public void Clear()
    {

    }

    public List<NoteData> SpatialQuery(CPPOGREnvelope env)
    {
        var r = new List<NoteData>();
        Enyim.Collections.Envelope e = new Enyim.Collections.Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY);
        spatialquery.Find(e, true, ref r);
        return r;
    }


    void ReadFeature2Tree()
    {
        Envelope env = new Envelope();
        layer.GetExtent(env, 1);
        mapenv = new Enyim.Collections.Envelope(env.MinX, env.MinY, env.MaxX, env.MaxY);
        env.Dispose();


//         var layerdef = layer.GetLayerDefn();
//         var c = layerdef.GetFieldCount();
//         for (int i = 0; i < c; i++)
//         {
//             var fielddef = layerdef.GetFieldDefn(i);
//             System.Diagnostics.Debug.Assert(fielddef != null);
//  
// 
//             Debug.Log(fielddef.GetName());
//             Debug.Log(fielddef.GetTypeName());
//             Debug.Log(fielddef.GetWidth());
//         }


        layer.ResetReading();
        Feature feat;
        while ((feat = layer.GetNextFeature()) != null)
        {
//             var s = feat.GetFieldAsString("DLMCX");
//             Debug.Log(s);
            spatialquery.Insert(feat);
        }
    }

    public Enyim.Collections.Envelope GetEnvelpoe()
    {
        return mapenv;
    }

    public void SetSpatialFilterRect(Enyim.Collections.Envelope env)
    {
        currentenv = env;
        List<NoteData> lst = new List<NoteData>();
        spatialquery.Find(currentenv, false, ref lst);
    }

    public List<NoteData> GetSpatialResult()
    {
        return spatialquery.GetReslut();
    }

    public long CreateFeature(Geometry geom)
    {
        Feature feature = new Feature(layer.GetLayerDefn());
        if (feature.SetGeometry(geom) != 0)
        {
            return Ogr.OGRNullFID;
        }
        if (layer.CreateFeature(feature) != 0)
        {
            return Ogr.OGRNullFID;
        }
        long fid = feature.GetFID();
        return fid;
    }
}
