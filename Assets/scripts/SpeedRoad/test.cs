using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using System;
using DigitalRuby.FastLineRenderer;
using Vectrosity;


public class test : MonoBehaviour {
    VectorLine line;


    // Use this for initialization
    void Start () {
//         List<Vector3> lst = new List<Vector3>();
//         lst.Add(Vector3.zero);
//         lst.Add(new Vector3(10, 0, 0));
//         lst.Add(new Vector3(10, 0, -5));
//         lst.Add(new Vector3( 0, 0, -5));
// 
//         List<int> idx = new List<int>();
//         idx.Add(0);
//         idx.Add(1);
//         idx.Add(2);
//         idx.Add(0);
//         idx.Add(2);
//         idx.Add(3);
// 
// 
//         Mesh msh = new Mesh();
//         msh.vertices = lst.ToArray();
//         msh.triangles = idx.ToArray();
//         msh.RecalculateNormals();
//         msh.RecalculateBounds();
// 
//         gameObject.AddComponent(typeof(MeshRenderer));
//         MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
//         filter.mesh = msh;
//         GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
// 
// 
//         line = new VectorLine("text", new List<Vector3>(2), 100);
//         line.color = Color.white;
//         line.points3[0] = Vector3.zero;
//         line.points3[1] = new Vector3(10, 0, 0);

        // Transform t = new Transform();
        // t.Rotate(Vector3.right, 90);
        // line.drawTransform = transform;

        // transform.Rotate(Vector3.right, 90);
        // line.drawTransform = transform;
        


    }

    // Update is called once per frame
    void Update () {
     //   line.Draw3D();
    }


}
