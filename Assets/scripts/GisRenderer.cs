using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;

public class GisRenderer
{
    FastLineRenderer lineRenderer = null;
    FastLineRendererProperties props = null;


    public void Init(FastLineRenderer lineparent, float radius)
    {
        System.Diagnostics.Debug.Assert(lineparent != null);
        lineRenderer = FastLineRenderer.CreateWithParent(null, lineparent);
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
                    DrawPolygon(geo);
                }
                break;
            case wkbGeometryType.wkbLineString:
                {
                    DrawLineString(geo);
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

    void DrawLineString(Geometry geo)
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

    void DrawPolygon(Geometry geo)
    {
        Geometry linearRing = geo.GetGeometryRef(0);
        if (Ogr.GT_Flatten(linearRing.GetGeometryType()) == wkbGeometryType.wkbLineString)
        {
            int count = linearRing.GetPointCount();
            if (count >= 2)
            {
                count -= 1;
                Vector2[] vertices2D = new Vector2[count];
                double[] pt = new double[2];
                for (int i = 0; i < count; i++)
                {
                    linearRing.GetPoint(i, pt);
                    vertices2D[i] = new Vector2((float)pt[0], (float)pt[1]);
                }
                /*
                Triangulator tr = new Triangulator(vertices2D);
                int[] indices = tr.Triangulate();

                // Create the Vector3 vertices
                Vector3[] vertices = new Vector3[vertices2D.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 50);
                }

                // Create the mesh
                Mesh msh = new Mesh();
                msh.vertices = vertices;
                msh.triangles = indices;
                msh.RecalculateNormals();
                msh.RecalculateBounds();
                
                // Set up game object with mesh;
                var g = new GameObject(Time.deltaTime.ToString());
                g.AddComponent(typeof(MeshRenderer));
                MeshFilter filter = g.AddComponent(typeof(MeshFilter)) as MeshFilter;
                filter.mesh = msh;
                g.GetComponent<MeshRenderer>().material.color = Color.red;
                g.transform.parent = GisWrapper.polygonParent.transform;
                */
                DrawLineString(linearRing);
            }
        }
    }
}
