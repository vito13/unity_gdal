using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;

public class GisRenderer
{
    FastLineRenderer lineRenderer = null;
    FastLineRendererProperties props = null;

    public void Init(FastLineRenderer parent, float radius)
    {
        System.Diagnostics.Debug.Assert(parent != null);
        lineRenderer = FastLineRenderer.CreateWithParent(null, parent);
        lineRenderer.Material.EnableKeyword("DISABLE_CAPS");
        lineRenderer.SetCapacity(FastLineRenderer.MaxLinesPerMesh * FastLineRenderer.VerticesPerLine);
        props = new FastLineRendererProperties();
        props.Radius = radius;
    }

    public void DrawGeometry(Geometry geo)
    {
        System.Diagnostics.Debug.Assert(geo != null);
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

   
}
