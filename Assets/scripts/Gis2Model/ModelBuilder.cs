using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using System;

public class ModelBuilder
{
    static List<Material> materialArr = new List<Material>();

    public static void SetMaterial(Material[] arr)
    {
        materialArr.Clear();
        materialArr.AddRange(arr);
    }

    public static GameObject CreateModel(GisViewer viewer, Transform parent, Feature fea)
    {
        string dlmcx = "1";//fea.GetFieldAsString("DLMCX");
        int dlmxctype = int.Parse(dlmcx) - 1;
        /*
        建筑基底  1
        绿地      2
        马路牙    3
        人行道    4
        水面      5
        水泥路    6
        停车位    7
        装饰路    8
         */

        var geom = fea.GetGeometryRef(); // fea.GetGeometryRef().Buffer(1, 1).Simplify(0.5);
        viewer.TransformGeometry2View(ref geom);

        float ypos = 0;
        switch (dlmxctype)
        {
            case 0:
                ypos = UnityEngine.Random.Range(50.0f, 100.0f);
                ypos = UnityEngine.Random.Range(5.0f, 10.0f);
                break;
            case 1:
                ypos = 5;
                break;
            case 2:
                ypos = 8;
                break;
            case 3:
                ypos = 4;
                break;
            case 4:
                ypos = 3;
                break;
            case 5:
                ypos = 6;
                break;
            case 6:
                ypos = 6;
                break;
            case 7:
                ypos = 6;
                break;
           
            default:
                break;
        }



        // 创建顶面
        var name = "fea_" + fea.GetFID().ToString();
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
                    AttachModel(go, linestring, ypos, dlmxctype);
                }
                break;
            case wkbGeometryType.wkbLineString:
                {
                    AttachModel(go, geom, ypos, dlmxctype);
                }
                break;
            default:
                break;
        }
        geom.Dispose();
        return go;
    }

    static void AttachModel(GameObject top, Geometry ls, float ypos, int type)
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
            top.GetComponent<MeshRenderer>().material = materialArr[type];
            AttachSide(top, arr, ypos, type);
        }
    }

    static void AttachSide(GameObject top, Vector2[] arr, float ypos, int type)
    {
        var name = top.name + "_side";
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
            indices[m + 0] = basenum + 0;
            indices[m + 1] = basenum + 1;
            indices[m + 2] = basenum + 2 >= vertices.Length ? 0 : basenum + 2;
            indices[m + 3] = basenum + 2 >= vertices.Length ? 0 : basenum + 2;
            indices[m + 4] = basenum + 1;
            indices[m + 5] = basenum + 3 >= vertices.Length ? 1 : basenum + 3;
        }
//         for (int i = 0; i < indices.Length; i+=3)
//         {
//             string s = string.Format("{0}, {1}, {2}", indices[i], indices[i+1], indices[i+2]);
//             Debug.Log(s);
//         }
       
        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        side.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = side.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        side.GetComponent<MeshRenderer>().material = materialArr[type];
    }
}
