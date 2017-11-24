using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testtexture2d : MonoBehaviour {
    [SerializeField]
    Texture2D tex;
	// Use this for initialization
	void Start () {
        test();

    }
	
	// Update is called once per frame
	void Update () {
		
	}


    void test()
    {
        // 构建顶点数组
        Vector3[] ptarr = new Vector3[6];
        ptarr[0] = new Vector3(0,  10, 0);
        ptarr[1] = new Vector3(0,  0, 0);
        ptarr[2] = new Vector3(10, 10, 0);
        ptarr[3] = new Vector3(10, 0, 0);
        ptarr[4] = new Vector3(20, 10, 0);
        ptarr[5] = new Vector3(20, 0, 0);

 
        // 构建索引数组
        List<int> idx = new List<int>();
        for (int i = 0; i < ptarr.Length - 2; i += 2)
        {
            idx.Add(i + 0);
            idx.Add(i + 2);
            idx.Add(i + 3);
            idx.Add(i + 0);
            idx.Add(i + 3);
            idx.Add(i + 1);
        }

        Vector2[] uv = {
            new Vector2(0, 1), new Vector2(0, 0),
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(1, 1), new Vector2(1, 0)
        };

        Mesh msh = new Mesh();
        msh.vertices = ptarr;
        msh.triangles = idx.ToArray();
        msh.uv = uv;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        gameObject.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
       
        var te = new Texture2D(1, 128);
        for (int i = 0; i < 128; i++)
        {
            if (i % 10 == 0)
            {
                te.SetPixel(0, i, Color.red);
            }
            else
            {
                te.SetPixel(0, i, Color.blue);
            }
        }
        te.Apply();

        gameObject.GetComponent<MeshRenderer>().material.mainTexture = tex;


    }
}
