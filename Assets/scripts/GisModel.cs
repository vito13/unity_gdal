using System.Collections;
using System.Collections.Generic;
using OSGeo.OGR;
using UnityEngine;

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
        System.Diagnostics.Debug.Assert(ds != null);
        System.Diagnostics.Debug.Assert(ds.GetLayerCount() > 0);
        layer = ds.GetLayerByIndex(0);
        System.Diagnostics.Debug.Assert(layer != null);
        

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

        layer.ResetReading();
        Feature feat;
        while ((feat = layer.GetNextFeature()) != null)
        {
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
