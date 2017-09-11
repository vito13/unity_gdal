using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using DigitalRuby.FastLineRenderer;
using UnityEngine;
using UnityEngine.UI;


public class usegis : MonoBehaviour
{
    [SerializeField]
    FastLineRenderer defaultRenderer = null;
    [SerializeField]
    FastLineRenderer selectionRenderer = null;
    [SerializeField]
    Text stateinfo;
    [SerializeField]
    GameObject polygonParent;


    GisWrapper gis = null;
    Vector3 mouseDown = Vector3.zero;
    float t = 0;

    void Awake()
    {
        testpy();
        GisWrapper.polygonParent = polygonParent;
        Load();
    }


    void Update()
    {
        if (gis != null)
        {
            Transmit();
            stateinfo.text = gis.GetStateInfo();
        }

    }

    public void SetHandTool()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISHand);
    }
    public void SetPointTool()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISPoint);
    }
    public void SetPolylineTool()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISPolyline);
    }
    public void SetPolygonTool()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISPolygon);
    }


    void Transmit()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gis.OnButtonDown();
        }
        else if (Input.GetMouseButton(0))
        {
            gis.OnButton();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            gis.OnButtonUp();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            gis.OnDblClk();
        }
        gis.OnMove();

        var sw = Input.GetAxis("Mouse ScrollWheel");
        if (sw != 0)
        {
            gis.OnWheel(sw > 0 ? true : false);
        }
    }

    public void FullExtend()
    {
        if (gis != null)
        {
            gis.FullExtent();
        }
    }

    public void Find(Vector2[] v)
    {
        if (gis != null)
        {
            var r = gis.GetFeatureInRange(v[0], v[1]);
        }
    }

    public void Load()
    {
        Clear();
        gis = new GisWrapper();
        gis.Init(defaultRenderer, selectionRenderer);
        gis.LoadFile("D:\\000testdata\\hgd\\line.shp");
        // gis.LoadFile("D:\\000testdata\\hgd\\poly.shp");
        FullExtend();
        gis.Redraw();
    }

    public void Clear()
    {
        if (gis != null)
        {
            gis.CleanCanvas();
        }
        gis = null;
    }

    void testpy()
    {
        var script = Resources.Load<TextAsset>("aaa").text; // resources内
        var scriptEngine = IronPython.Hosting.Python.CreateEngine();
        var scriptScope = scriptEngine.CreateScope();
        var scriptSource = scriptEngine.CreateScriptSourceFromString(script);

        scriptSource.Execute(scriptScope);

    }
}
