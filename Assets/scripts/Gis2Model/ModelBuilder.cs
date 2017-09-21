using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using System;
using Vectrosity;

public class ModelBuilder
{
    static VectorLine line;

  

    static int index = 0;
    public static GameObject CreateModel(GisViewer viewer, Transform parent, Feature fea)
    {
        var geom = fea.GetGeometryRef().Clone(); //.SimplifyPreserveTopology(10);
        viewer.TransformGeometry2View(ref geom);

        float ypos = UnityEngine.Random.Range(20.0f, 50.0f);

        // 创建顶面
        var name = fea.GetFID().ToString();
        GameObject go = new GameObject(name);
        go.transform.parent = parent;

        wkbGeometryType t = Ogr.GT_Flatten(geom.GetGeometryType());
        switch (t)
        {
            case wkbGeometryType.wkbUnknown:
                break;
            case wkbGeometryType.wkbPoint:
                break;
            case wkbGeometryType.wkbPolygon:
                {
                    Geometry linestring = geom.GetGeometryRef(0);
                    AttachModel(go, linestring, ypos);
                }
                break;
            case wkbGeometryType.wkbLineString:
                {
                    AttachModel(go, geom, ypos);
                }
                break;
            default:
                break;
        }
        geom.Dispose();
        // 创建侧面
        return go;
    }

    static void AttachModel(GameObject top, Geometry ls, float ypos)
    {
        int count = ls.GetPointCount();
        if (count >= 2)
        {
            count -= 1;
            Vector2[] arr = new Vector2[count];
            double[] pt = new double[2];
            for (int i = 0; i < count; i++)
            {
                ls.GetPoint(i, pt);
                arr[i] = new Vector2((float)pt[0], (float)pt[1]);
            }

            Triangulator tr = new Triangulator(arr);
            int[] indices = tr.Triangulate();

            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[arr.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(arr[i].x, ypos, arr[i].y);
            }

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();

            top.AddComponent(typeof(MeshRenderer));
            MeshFilter filter = top.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;
            top.GetComponent<MeshRenderer>().material.color = Color.gray;


//             line = new VectorLine("Selection", new List<Vector2>(), null, 1.0f, LineType.Continuous);
//             line.capLength = 0.5f;
//             line.SetColor(Color.white);
//             for (int i = 0; i < arr.Length; i++)
//             {
//                 line.points2.Add(arr[i]);
//             }
//             line.Draw();


            // side model
            AttachSide(top, arr, ypos);
        }
    }

    static void AttachSide(GameObject top, Vector2[] arr, float ypos)
    {
        // Debug.Log(arr.Length);


//         Vector2[] arr = new Vector2[3];
//         arr[0] = new Vector2(0, 0);
//         arr[1] = new Vector2(50, 0);
//         arr[2] = new Vector2(0, 50);

        var name = top.name + "side";
        GameObject side = new GameObject(name);
        side.transform.parent = top.transform;

        /*
           4
           /\
          /| \
         / |  \
        /  |   \
      0 ---|---- 2
       |   |    |
       |   |    |
       |   /\   |
       |  /5 \  |
       | /    \ |
       |/      \|
        -------- 
        1       3
       */
        Vector3[] vertices = new Vector3[arr.Length * 2];
        for (int m = 0; m < vertices.Length; m += 2)
        {
            vertices[m] = new Vector3(arr[m / 2].x, ypos, arr[m / 2].y);
            vertices[m + 1] = new Vector3(arr[m / 2].x, 0, arr[m / 2].y);
        }

        int[] indices = new int[arr.Length * 6];
 /*
        indices[0] = 0;
        indices[1] = 2;
        indices[2] = 1;
        indices[3] = 1;
        indices[4] = 2;
        indices[5] = 3;

        indices[6] = 2;
        indices[7] = 4;
        indices[8] = 3;
        indices[9] = 3;
        indices[10] = 4;
        indices[11] = 5;

        indices[12] = 4;
        indices[13] = 0;
        indices[14] = 5;
        indices[15] = 5;
        indices[16] = 0;
        indices[17] = 1;
       
        */
        for (int m = 0; m < indices.Length; m += 6)
        {
             
        // 需要顺时针才能看到外面，逆时针是里面    
        //    0     2     4     0
        //    -------------------
        //    |   / |   / |   / |
        //    |  /  |  /  |  /  |
        //    | /   | /   | /   |
        //    -------------------
        //    1     3     5     1
            
            var basenum = m / 3;
//             indices[m + 0] = basenum + 0;
//             indices[m + 1] = basenum + 2 >= vertices.Length ? 0 : basenum + 2;
//             indices[m + 2] = basenum + 1;
//             indices[m + 3] = basenum + 1;
//             indices[m + 4] = basenum + 2 >= vertices.Length ? 0 : basenum + 2;
//             indices[m + 5] = basenum + 3 >= vertices.Length ? 1 : basenum + 3;

            indices[m + 0] = basenum + 0;
            indices[m + 1] = basenum + 1;
            indices[m + 2] = basenum + 2 >= vertices.Length ? 0 : basenum + 2;
            indices[m + 3] = basenum + 2 >= vertices.Length ? 0 : basenum + 2;
            indices[m + 4] = basenum + 1;
            indices[m + 5] = basenum + 3 >= vertices.Length ? 1 : basenum + 3;
        }
        for (int i = 0; i < indices.Length; i+=3)
        {
            string s = string.Format("{0}, {1}, {2}", indices[i], indices[i+1], indices[i+2]);
            Debug.Log(s);
        }
       
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        side.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = side.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        side.GetComponent<MeshRenderer>().material.color = Color.gray;
    }
}
