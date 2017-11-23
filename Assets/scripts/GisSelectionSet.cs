using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;
using UnityEngine.Assertions;

public class GisSelectionSet
{
    List<NoteData> lst = new List<NoteData>();
    FastLineRenderer lineRenderer = null;
    FastLineRendererProperties props = null;
    float Radius = 0;

    public void Init(FastLineRenderer parent, float radius)
    {
        Assert.IsNotNull(parent);
        lineRenderer = FastLineRenderer.CreateWithParent(null, parent);
        lineRenderer.Material.EnableKeyword("DISABLE_CAPS");
        lineRenderer.SetCapacity(FastLineRenderer.MaxLinesPerMesh * FastLineRenderer.VerticesPerLine);
        props = new FastLineRendererProperties();
        props.Radius = radius;
    }

    public void Add(NoteData data)
    {
        lst.Add(data);
    }

    public void Remove()
    {

    }

    public void Clear()
    {
        lst.Clear();
    }

    void DrawGeometry(Geometry geo)
    {
        wkbGeometryType t = Ogr.GT_Flatten(geo.GetGeometryType());
        switch (t)
        {
            case wkbGeometryType.wkbUnknown:
                break;
            case wkbGeometryType.wkbPoint:
                break;
            case wkbGeometryType.wkbPolygon:
                {
                    Geometry linestring = geo.GetGeometryRef(0);
                    if (Ogr.GT_Flatten(linestring.GetGeometryType()) == wkbGeometryType.wkbLineString)
                    {
                        DrawGeometry(linestring);
                    }
                }
                break;
            case wkbGeometryType.wkbLineString:
                {
                    int count = geo.GetPointCount();
                    if (count >= 2)
                    {
                        double[] pt = new double[2];
                        for (int i = 0; i < count - 1; i++)
                        {
                            geo.GetPoint(i, pt);
                            props.Start = new Vector2((float)pt[0], (float)pt[1]);
                            lineRenderer.AppendLine(props);
                        }
                        geo.GetPoint(count - 1, pt);
                        props.Start = new Vector2((float)pt[0], (float)pt[1]);
                        lineRenderer.EndLine(props);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void Redraw(GisViewer v)
    {
        lineRenderer.Reset();
        props.Radius = 1.5f * (float)v.GetResolution();
        foreach (var item in lst)
        {
            var geo = item.fea.GetGeometryRef().Clone();
            v.TransformGeometry2View(ref geo);
            DrawGeometry(geo);
            geo.Dispose();
        }
        lineRenderer.Apply();
    }

    public IEnumerable<long> GetSelection()
    {
        List<long> r = new List<long>();
        for (int i = 0; i < lst.Count; i++)
        {
            r.Add(lst[i].fea.GetFID());
        }
        return r;
    }
}
