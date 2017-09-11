using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public class GisPolygonTool : GisOperatingTool
{
    Mesh msh;
    List<Vector2> vertices2D;
    int step;

    protected override void init()
    {
        msh = new Mesh();
        vertices2D = new List<Vector2>();
        var g = new GameObject(Time.deltaTime.ToString());
        g.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = g.AddComponent(typeof(MeshFilter)) as MeshFilter;
        g.GetComponent<MeshRenderer>().material.color = Color.green;
        g.transform.parent = GisWrapper.polygonParent.transform;
        type = OperatingToolType.GISPolygon;
        filter.mesh = msh;

        line = new VectorLine(ToString(), new List<Vector2>(), null, 1.0f, LineType.Continuous);
        line.SetColor(Color.white);

        Reset();
    }

    public override void OnButtonDown()
    {
        if (step == 0)
        {
            line.points2.Add(Input.mousePosition);
            line.points2.Add(Input.mousePosition);
            vertices2D[0] = Input.mousePosition;
            vertices2D[1] = Input.mousePosition;
            vertices2D[2] = Input.mousePosition;
            step++;
        }
        else if (step == 1)
        {
            line.points2.Add(Input.mousePosition);
            vertices2D[1] = Input.mousePosition;
            vertices2D[2] = Input.mousePosition;
            step++;
        }
        else if (step == 2)
        {
            line.points2.Add(Input.mousePosition);
            vertices2D[2] = Input.mousePosition;
            vertices2D.Add(Input.mousePosition);
            step++;
        }
        else
        {
            line.points2.Add(Input.mousePosition);
            vertices2D.Add(Input.mousePosition);
        }
    }

    public override void OnMove()
    {
        if (step != 0)
        {
            line.points2[line.points2.Count - 1] = Input.mousePosition;
            line.Draw();
        }

        if (step >= 2)
        {
            vertices2D[vertices2D.Count - 1] = Input.mousePosition;
            Triangulator tr = new Triangulator(vertices2D.ToArray());
            int[] indices = tr.Triangulate();
            Vector3[] vertices = new Vector3[vertices2D.Count];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 50);
            }
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
        }
    }

    public override void OnDblClk()
    {
        Reset();
    }

    public override void Reset()
    {
        line.points2.Clear();
        vertices2D.Clear();
        vertices2D.Add(Vector2.zero);
        vertices2D.Add(Vector2.zero);
        vertices2D.Add(Vector2.zero);
        step = 0;
    }
}
