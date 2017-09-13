using System.Collections;
using System.Collections.Generic;
using Enyim.Collections;
using UnityEngine;
using OSGeo.OGR;


public class NoteData
{
    public Feature fea;
}

public class GisSpatialQuery{
    RTree<RTreeNode<NoteData>> tree = new RTree<RTreeNode<NoteData>>();
    List<NoteData> lstFindResult = new List<NoteData>();  // 查询结果


    public void Clear()
    {
        // 需要Dispose所有feature
        tree.Clear();
        lstFindResult.Clear();
    }

    public bool Insert(Feature f)
    {
        bool result = false;
        NoteData data = new NoteData();
        data.fea = f;
        var geo = data.fea.GetGeometryRef();
        System.Diagnostics.Debug.Assert(geo != null);
        OSGeo.OGR.Envelope envogr = new OSGeo.OGR.Envelope();
        geo.GetEnvelope(envogr);
        Enyim.Collections.Envelope env = new Enyim.Collections.Envelope(envogr.MinX, envogr.MinY, envogr.MaxX, envogr.MaxY);
        envogr.Dispose();
        RTreeNode<NoteData> node = new RTreeNode<NoteData>(data, env);
        tree.Insert(node, env);
        result = true;
        return result;
    }

    public bool Remove(int fid)
    {
        bool result = false;
        return result;
    }

    public int Find(Enyim.Collections.Envelope env, bool getdata, ref List<NoteData> lst)
    {
        int result = 0;
        var r = tree.Search(env);
        if (getdata == true)
        {
            lst.Clear();
            foreach (var item in r)
            {
                lst.Add(item.Data.Data);
            }
            result = lst.Count;
        }
        else
        {
            lstFindResult.Clear();
            foreach (var item in r)
            {
                lstFindResult.Add(item.Data.Data);
            }
            result = lstFindResult.Count;
        }
        return result;  
    }

    public List<NoteData> GetReslut()
    {
        return lstFindResult;
    }

}
