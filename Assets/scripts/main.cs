using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using OSGeo.OGR;
using System;
using UnityEngine.UI;
using DigitalRuby.FastLineRenderer;
using Newtonsoft.Json;
using Enyim.Collections;


public class main : MonoBehaviour
{
    [SerializeField]
    FastLineRenderer LineRenderer;
    [SerializeField]
    float Radius = 0.02f;

    [SerializeField]
    Text txt;

    RTree<int> tree = new RTree<int>();
    DataSource ds = null;

    // Use this for initialization
    void Awake()
    {
        LoadFile();
        /*
        {
            Enyim.Collections.Envelope env = new Enyim.Collections.Envelope(0, 0, 100, 100);
            RTreeNode<int> node = new RTreeNode<int>(0, env);
            tree.Insert(node);
        }
        {
            Enyim.Collections.Envelope env = new Enyim.Collections.Envelope(-100, -100, 0, 0);
            RTreeNode<int> node = new RTreeNode<int>(1, env);
            tree.Insert(node);
        }
        {
            Enyim.Collections.Envelope env = new Enyim.Collections.Envelope(-100, 0, 0, 100);
            RTreeNode<int> node = new RTreeNode<int>(2, env);
            tree.Insert(node);
        }
        {
            Enyim.Collections.Envelope env = new Enyim.Collections.Envelope(0, -100, 100, 0);
            RTreeNode<int> node = new RTreeNode<int>(3, env);
            tree.Insert(node);
        }

        {
            Enyim.Collections.Envelope env = new Enyim.Collections.Envelope(-100, -100, -1, -1);
            var result = tree.Search(env);
            foreach (var item in result)
            {
                Debug.Log(item.Data);
            }
        }
      */
    }

    void RenderTest()
    {
        
        FastLineRenderer r = FastLineRenderer.CreateWithParent(null, LineRenderer);
        r.Material.EnableKeyword("DISABLE_CAPS");
        r.SetCapacity(FastLineRenderer.MaxLinesPerMesh * FastLineRenderer.VerticesPerLine);
        FastLineRendererProperties props = new FastLineRendererProperties();
        props.Radius = Radius;
 
        props.Start = Vector3.zero;
        r.AppendLine(props);
        props.Start = new Vector3(200, 0, 0);
        r.AppendLine(props);
        props.Start = new Vector3(200, 200, 0);
        r.EndLine(props);


        props.Start = new Vector3(100, 100, 0);
        r.AppendLine(props);
        props.Start = new Vector3(100, 200, 0);
        r.AppendLine(props);
        props.Start = new Vector3(0, 200, 0);
        r.EndLine(props);
        r.Apply();
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            // RenderTest();
            Render();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            FastLineRenderer.ResetAll();
        }
    }

    void LoadFile()
    {
        string fn = "D:\\000testdata\\hgd\\line.shp";
        Ogr.RegisterAll();
        ds = Ogr.Open(fn, 0);
    }

    void Render()
    {
        FastLineRenderer r = FastLineRenderer.CreateWithParent(null, LineRenderer);
        r.Material.EnableKeyword("DISABLE_CAPS");
        r.SetCapacity(FastLineRenderer.MaxLinesPerMesh * FastLineRenderer.VerticesPerLine);
        FastLineRendererProperties props = new FastLineRendererProperties();
        props.Radius = 0.02f;

        OSGeo.OGR.Driver drv = ds.GetDriver();
        for (int iLayer = 0; iLayer < ds.GetLayerCount(); iLayer++)
        {
            Layer layer = ds.GetLayerByIndex(iLayer);
            layer.ResetReading();
            Feature feat;
            while ((feat = layer.GetNextFeature()) != null)
            {
                Geometry geom = feat.GetGeometryRef();
                if (geom != null)
                {
                    wkbGeometryType t = Ogr.GT_Flatten(geom.GetGeometryType());
                    switch (t)
                    {
                        case wkbGeometryType.wkbUnknown:
                            break;
                        case wkbGeometryType.wkbPoint:
                            break;
                        case wkbGeometryType.wkbLineString:
                            DrawPolyline(r, props, geom);
                            break;
                        default:
                            break;
                    }

                }
                feat.Dispose();
            }
        }
        r.Apply();
    }

    private byte RandomByte()
    {
        return (byte)UnityEngine.Random.Range(0, byte.MaxValue);
    }

    private Vector3 RandomVelocity(float c)
    {
        return new Vector3(UnityEngine.Random.Range(-c, c), UnityEngine.Random.Range(-c, c), UnityEngine.Random.Range(-c, c));
    }

    void DrawPolyline(FastLineRenderer render, FastLineRendererProperties prop, Geometry geo)
    {
        string json = geo.ExportToJson(null);
        var definition = new {
            type = "",
            coordinates = new List<float[]>()
        };
        var obj = JsonConvert.DeserializeAnonymousType(json, definition);

        var pt = obj.coordinates[0];
        prop.Start = new Vector3(pt[0], pt[1], pt[2]);
        render.AppendLine(prop);
        for (int i = 1; i < obj.coordinates.Count - 1; i++)
        {
            pt = obj.coordinates[i];
            prop.Start = new Vector3(pt[0], pt[1], pt[2]);
            render.AppendLine(prop);
        }
        pt = obj.coordinates[obj.coordinates.Count - 1];
        prop.Start = new Vector3(pt[0], pt[1], pt[2]);
        render.EndLine(prop);
    }
}
