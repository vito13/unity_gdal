using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;

public class GisSelectionSet{
    List<Feature> lst = new List<Feature>();
    FastLineRenderer lineRenderer = null;
    FastLineRendererProperties props = null;
    public float Radius = 0.02f;
    GisViewer viewer = null;


    public void Init(FastLineRenderer parent, GisViewer v)
    {
        System.Diagnostics.Debug.Assert(parent != null);
        lineRenderer = FastLineRenderer.CreateWithParent(null, parent);
        lineRenderer.Material.EnableKeyword("DISABLE_CAPS");
        lineRenderer.SetCapacity(FastLineRenderer.MaxLinesPerMesh * FastLineRenderer.VerticesPerLine);
        props = new FastLineRendererProperties();
        props.Radius = Radius;
        viewer = v;
    }

    public void Add(Feature fea)
    {
        lst.Add(fea);
    }

    public void Add(IEnumerable<Feature> feas)
    {
        lst.AddRange(feas);
    }

    public void Remove()
    {

    }

    public void Clear()
    {
        lst.Clear();
    }

    public void Redraw()
    {
//         lineRenderer.Reset();
//         foreach (var item in lst)
//         {
//             Geometry geom = item.GetGeometryRef();
//             var lst = utils.GetGeometryPoints(geom);
// 
//             List<Vector3> result = new List<Vector3>();
//             for (int i = 0; i < lst.Count; i++)
//             {
//                 var pt = viewer.MapToView(lst[i]);
//                 result.Add(pt);
//             }
//                  
//             if (result.Count >= 2)
//             {
//                 for (int m = 0; m < result.Count - 1; m++)
//                 {
//                     props.Start = result[m];
//                     lineRenderer.AppendLine(props);
//                 }
//                 props.Start = result[result.Count - 1];
//                 lineRenderer.EndLine(props);
//             }
//         }
//         lineRenderer.Apply();
    }
}
