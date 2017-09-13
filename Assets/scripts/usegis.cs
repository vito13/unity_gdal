using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using DigitalRuby.FastLineRenderer;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    [SerializeField]
    Toggle sqtype; // 用于监测当前空间查询的关系方式，对应SpatialRelation_TYPE枚举


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

        if (Input.GetKeyDown(KeyCode.A))
        {
            gis.Redraw();

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
    public void SetBoxSelect()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISBoxSelect);
    }
    public void SetPointSelect()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISPointSelect);
    }
    public void SetCircleSelect()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISCircleSelect);
    }
    public void SetPolygonSelect()
    {
        if (gis != null)
            gis.SetCurrentTool(OperatingToolType.GISPolygonSelect);
    }

    public void SetSpatialRelation()
    {
        if (gis != null)
        {
            if (sqtype.isOn)
            {
                gis.SetSpatialRelationl(SpatialRelation_TYPE.hhhwSRT_Within);
            }
            else
            {
                gis.SetSpatialRelationl(SpatialRelation_TYPE.hhhwSRT_Intersect);
            }
            Debug.Log(GisOperatingToolSet.sqtype);
        }
        
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

    public IEnumerable<long> GetSelection()
    {
        IEnumerable<long> r = null;
        if (gis != null)
        {
            r = gis.GetSelection();
        }
        return r;
    }

    public void Load()
    {
        Clear();
        gis = new GisWrapper();
        gis.Init(defaultRenderer, selectionRenderer);
       // gis.LoadFile("D:\\000testdata\\testshp11\\bou2_4p.shp");
        gis.LoadFile("D:\\000testdata\\hgd\\line.shp");
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
