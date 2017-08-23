using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.FastLineRenderer;
using Newtonsoft.Json;

public class GisRenderer{
    FastLineRenderer lineRenderer = null;
    FastLineRendererProperties props = null;
    public float Radius = 0.02f;
    GisParams gp = null;

    public void Init(FastLineRenderer parent, GisParams par)
    {
        System.Diagnostics.Debug.Assert(parent != null);
        lineRenderer = FastLineRenderer.CreateWithParent(null, parent);
        lineRenderer.Material.EnableKeyword("DISABLE_CAPS");
        lineRenderer.SetCapacity(FastLineRenderer.MaxLinesPerMesh * FastLineRenderer.VerticesPerLine);
        props = new FastLineRendererProperties();
        props.Radius = Radius;
        gp = par;
    }

    public void DrawGeometry(string json)
    {
        var definition = new
        {
            type = "",
            coordinates = new List<float[]>()
        };
        var obj = JsonConvert.DeserializeAnonymousType(json, definition);
        var pts = Array2Vector3(obj.coordinates);
        var lst = TransformMyPoint(pts);

        if (lst.Count >= 2)
        {
            for (int m = 0; m < lst.Count - 1; m++)
            {
                props.Start = lst[m];
                lineRenderer.AppendLine(props);
            }
            props.Start = lst[lst.Count - 1];
            lineRenderer.EndLine(props);
        }
    }

    public void Clear()
    {
        lineRenderer.Reset();
    }

    public void BeginDraw()
    {
        
    }

    public void EndDraw()
    {
        lineRenderer.Apply();
    }

    static List<Vector3> Array2Vector3(List<float[]> lst)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < lst.Count; i++)
        {
            Vector3 v = Vector3.zero;
            for (int m = 0; m < lst[i].Length; m++)
            {
                v[m] = lst[i][m];
            }
            result.Add(v);
        }

        return result;
    }

    List<Vector3> TransformMyPoint(List<Vector3> lst)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < lst.Count; i++)
        {
            var pt = gp.MapToView(lst[i]);
            result.Add(pt);
        }
        return result;
    }
}
