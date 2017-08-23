using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GisViewer : MonoBehaviour {
    [SerializeField]
    FastLineRenderer parent;
    [SerializeField]
    float ratioZooming = 1.3f;


    GisRenderer renderer = null;
    GisModel model = null;
    Vector3 mouseDown = Vector3.zero;
    GisParams gisparam = new GisParams();

    void Awake () {
        Load();
    }
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Test();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            DrawScene();
        }
//         Zooming();
//         Offseting();
    }

    void Load()
    {
        Ogr.RegisterAll();
        model = new GisModel();
        //string fn = "D:\\000testdata\\testshp11\\mviewtest1line.shp";
        string fn = "D:\\000testdata\\hgd\\line.shp";
        model.Open(fn);


        renderer = new GisRenderer();
        Rect env = Rect.zero;
        model.GetEnvelpoe(ref env);
       
        Rect clientSize = Rect.zero;

        clientSize.position = new Vector2(-Screen.width / 2, -Screen.height / 2);
        clientSize.width = Screen.width;
        clientSize.height = Screen.height;
        gisparam.ResetResolution(env, clientSize);
        renderer.Init(parent, gisparam);
    }

    void DrawScene()
    {
        renderer.Clear();
        model.ResetFeatureIterator();
        renderer.BeginDraw();
        string json = "";
        while ((json = model.GetNextFeatureGeometryJson()).Length > 0)
        {
            renderer.DrawGeometry(json);
        }
        renderer.EndDraw();
    }

    void Init()
    {
           
    }
    
    void Zooming()
    {
        var sw = Input.GetAxis("Mouse ScrollWheel");
        float dis = 0;
       
        if (sw < 0)
        {
            var bak = Camera.main.orthographicSize;
            Camera.main.orthographicSize *= ratioZooming;
            dis = Camera.main.orthographicSize - bak;
        }
        else if (sw > 0)
        {
            var bak = Camera.main.orthographicSize;
            Camera.main.orthographicSize /= ratioZooming;
            dis = Camera.main.orthographicSize - bak;
        }

        if (dis != 0)
        {

        }
    }


    void Offseting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Camera.main.transform.Translate(-(Input.mousePosition - mouseDown));
            mouseDown = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            mouseDown = Vector3.zero;
        }

    }

    void Test()
    {
        Vector3 worldPos1 = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 worldPos2 = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -Camera.main.transform.position.z));
        Debug.Log(worldPos1.x + "," + worldPos1.y + "," + worldPos1.z);
        Debug.Log(worldPos2.x + "," + worldPos2.y + "," + worldPos2.z);
    }
}
