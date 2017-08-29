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
        ds = Ogr.Open(fname, 0);
        System.Diagnostics.Debug.Assert(ds != null);
        System.Diagnostics.Debug.Assert(ds.GetLayerCount() > 0);
        layer = ds.GetLayerByIndex(0);
        System.Diagnostics.Debug.Assert(layer != null);
        

        spatialquery = new GisSpatialQuery();
        spatialquery.Clear();
        ReadFeature2Tree();
        currentenv = new Enyim.Collections.Envelope();
    }

    public IEnumerable<NoteData> SpatialQuery(Enyim.Collections.Envelope env)
    {
        var r = new List<NoteData>();
        spatialquery.Find(env, true, ref r);
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

    public List<NoteData> GetQueryResult()
    {
        return spatialquery.GetReslut();
    }
}
