using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GisWrapper : IMouseAction{
    GisViewer viewer = null;
    GisModel model = null;
    GisRenderer fastrenderer = null;
    GisSelectionSet ss = null;
    GisOperatingToolSet optool = null;
    static public GameObject polygonParent;


    public void Init (FastLineRenderer defaultRenderer, FastLineRenderer selectionRenderer) {
        Ogr.RegisterAll();
        model = new GisModel();
        viewer = new GisViewer();
        viewer.Init();
        fastrenderer = new GisRenderer();
        fastrenderer.Init(defaultRenderer, 0.02f);

        ss = new GisSelectionSet();
        ss.Init(selectionRenderer, 0.2f);
        optool = new GisOperatingToolSet();
        optool.Init(viewer);
    }
	

    public void LoadFile(string fname)
    {
        model.Open(fname);
    }

    public void FullExtent()
    {
        var env = model.GetEnvelpoe();
        CPPOGREnvelope viewsize = new CPPOGREnvelope(0, 0, Screen.width, Screen.height);

        viewer.ResetResolution(utils.ECEnvlope2CPP(env), viewsize);
        viewer.Init();
        model.SetSpatialFilterRect(env);
        viewer.ResetStandardRate();
    }

    public void Translation(Vector2 offset)
    {
        if (utils.PositionInView(Input.mousePosition) && offset != Vector2.zero)
        {
            viewer.Translate(new Vector2D(offset.x, offset.y));
        }
    }

    public void CleanCanvas()
    {
        fastrenderer.Clear();
    }

    public void Redraw()
    {
        fastrenderer.Clear();
        fastrenderer.BeginDraw();

        var lst = model.GetCurrentViewing();
        Geometry geo = null;
        for (int i = 0; i < lst.Count; i++)
        {
            geo = lst[i].fea.GetGeometryRef().Clone();
            viewer.TransformGeometry2View(ref geo);
            fastrenderer.DrawGeometry(geo);
            geo.Dispose();
        }
        fastrenderer.EndDraw();
    }

    public Vector2D MapToView(double x, double y)
    {
        return viewer.MapToView(x, y);
    }

    public Vector2D ViewToMap(float x, float y)
    {
        return viewer.ViewToMap(x, y);
    }

    public string GetStateInfo()
    {
        var env = model.GetEnvelpoe();
        float h = Camera.main.orthographicSize * 2;
        float w = Camera.main.aspect * h;


        string result = string.Format(
            "窗口坐标: {0}; 地图坐标: {1}; " +
            "\n比例尺: {2}; 地图范围: {3}",
        //             "\n窗口中心点: {2}对应的地图坐标: {3}" +
        //             "\n窗口尺寸: ({4}, {5}); 地图范围: {6}" +
        //             "\n比例尺: {7}; 摄像机Size: {8}" +
        //             "\n当前呈现地图范围： {9}\n摄像机位置: {10}; 范围: (xmin: {11}, xmax: {12}, ymin: {13}, ymax: {14})",
        Input.mousePosition.ToString(), ViewToMap(Input.mousePosition.x, Input.mousePosition.y).ToString(),
            viewer.GetResolution(), viewer.GetCurrentMapRect().ToString()
            //             viewer.GetSeeCenter().ToString(), ViewToMap(viewer.GetSeeCenter().x, viewer.GetSeeCenter().y).ToString(),
            //             Screen.width, Screen.height, env.ToString(),
            //             viewer.GetResolution(), Camera.main.orthographicSize,
            //             viewer.GetCurrentMapRect().ToString(),
            //             Camera.main.transform.position.ToString(),
            //             Camera.main.transform.position.x - w * 0.5f,
            //             Camera.main.transform.position.x + w * 0.5f,
            //             Camera.main.transform.position.y - h * 0.5f,
            //             Camera.main.transform.position.y + h * 0.5f
            );
        return result;
    }

    public IEnumerable<int> GetFeatureInRange(Vector2 vmin, Vector2 vmax)
    {
        List<int> lst = new List<int>();
        Vector2D ptmin = ViewToMap(vmin.x, vmin.y);
        Vector2D ptmax = ViewToMap(vmax.x, vmax.y);
        var r = model.SpatialQuery(new Enyim.Collections.Envelope(ptmin.x, ptmin.y, ptmax.x, ptmax.y));

        ss.Clear();
        foreach (var item in r)
        {
            ss.Add(item);
        }
        ss.Redraw(viewer);
        return lst;
    }

    public void OnButtonDown()
    {
        optool.OnButtonDown();
    }

    public void OnButton()
    {
        optool.OnButton();
    }

    public void OnButtonUp()
    {
        optool.OnButtonUp();
    }

    public void OnWheel(bool t)
    {
        optool.OnWheel(t);
    }

    public void OnMove()
    {
        optool.OnMove();
    }

    public void OnDblClk()
    {
        optool.OnDblClk();
    }

    public void SetCurrentTool(OperatingToolType t)
    {
        optool.SetCurrentType(t);
    }
}
