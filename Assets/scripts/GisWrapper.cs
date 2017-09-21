using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class GisWrapper : IMouseAction
{
    GisViewer viewer = null;
    GisModel model = null;
    GisRenderer fastrenderer = null;
    GisSelectionSet ss = null;
    GisOperatingToolSet optool = null;
    static public GameObject polygonParent;



    public void Init(FastLineRenderer defaultRenderer, FastLineRenderer selectionRenderer)
    {
        Ogr.RegisterAll();
        model = new GisModel();
        viewer = new GisViewer();
        viewer.Init();
        fastrenderer = new GisRenderer();
        fastrenderer.Init(defaultRenderer, 0.02f);

        ss = new GisSelectionSet();
        ss.Init(selectionRenderer, 0.2f);

        GisOperatingToolSet.SetHost(this);
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

        var lst = model.GetSpatialResult();
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

    public IEnumerable<long> GetSelection()
    {
        return ss.GetSelection();
    }

    
    void SurfaceSpatialQuery(List<Vector2D> lst, SpatialRelation_TYPE t)
    {
        Geometry poly = new Geometry(wkbGeometryType.wkbPolygon);
        Geometry lr = new Geometry(wkbGeometryType.wkbLinearRing);
        foreach (var item in lst)
        {
            lr.AddPoint_2D(item.x, item.y);
        }
        lr.CloseRings();
        poly.AddGeometryDirectly(lr);

        Envelope ogrenv = new Envelope();
        poly.GetEnvelope(ogrenv);
        CPPOGREnvelope env = new CPPOGREnvelope();
        env.MinX = ogrenv.MinX;
        env.MinY = ogrenv.MinY;
        env.MaxX = ogrenv.MaxX;
        env.MaxY = ogrenv.MaxY;
        var r = model.SpatialQuery(env);
        ogrenv.Dispose();

        foreach (var item in r)
        {
            var other = item.fea.GetGeometryRef();
            bool r2 = false;
            if (t == SpatialRelation_TYPE.hhhwSRT_Intersect)
            {
                r2 = utils.Intersects(poly, other);
            }
            else if (t == SpatialRelation_TYPE.hhhwSRT_Within)
            {
                r2 = utils.Within(poly, other);
            }
            if (r2)
            {
                ss.Add(item);
            }
        }
        poly.Dispose();
    }

    void PointSpatialQuery(Vector2D pt)
    {
        CPPOGREnvelope env = new CPPOGREnvelope();
        env.MinX = pt.x;
        env.MinY = pt.y;
        env.MaxX = pt.x;
        env.MaxY = pt.y;
        var r = model.SpatialQuery(env);

        Geometry point = new Geometry(wkbGeometryType.wkbPoint);
        point.SetPoint_2D(0, pt.x, pt.y);
        foreach (var item in r)
        {
            var other = item.fea.GetGeometryRef();
            bool r2 = utils.Within(other, point);
            if (r2)
            {
                ss.Add(item);
            }
        }
        point.Dispose();
    }

    void LineSpatialQuery(Vector2D pt1, Vector2D pt2)
    {
        Debug.Log("还未实现～～～");
    }

    public void SpatialQuery(Vector2[] arr)
    {
        ss.Clear();
        List<Vector2D> lst = new List<Vector2D>();
        for (int i = 0; i < arr.Length; i++)
        {
            var p1 = viewer.ViewToMap(arr[i].x, arr[i].y);
            lst.Add(p1);
        }
        switch (lst.Count)
        {
            case 1:
                PointSpatialQuery(lst[0]);
                break;
            case 2:
                LineSpatialQuery(lst[0], lst[1]);
                break;
            default:
                SurfaceSpatialQuery(lst, GisOperatingToolSet.sqtype);
                break;
        }
        ss.Redraw(viewer);
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
    public void CreateModel()
    {
        var lst = model.GetSpatialResult();
        for (int i = 0; i < lst.Count; i++)
        {
            if (lst[i].fea.GetFID() %5 != 1)
            {
                continue;
                // break;
            }
         
            ModelBuilder.CreateModel(viewer, polygonParent.transform, lst[i].fea);
        }
    }

    public void SetCurrentTool(OperatingToolType t)
    {
        optool.SetCurrentType(t);
    }

    public void SetSpatialRelationl(SpatialRelation_TYPE t)
    {
        GisOperatingToolSet.sqtype = t;
    }

    void CreateFeature_line(List<Vector2D> lst)
    {
        Geometry ls = new Geometry(wkbGeometryType.wkbLineString);
        foreach (var item in lst)
        {
            ls.AddPoint_2D(item.x, item.y);
        }
        var fid = model.CreateFeature(ls);
        Debug.Log(fid);
        ls.Dispose();
    }
    void CreateFeature_surface(List<Vector2D> lst)
    {

    }
    void CreateFeature_point(Vector2D pt)
    {

    }
    public void CreateFeature(Vector2[] arr)
    {
        List<Vector2D> lst = new List<Vector2D>();
        for (int i = 0; i < arr.Length; i++)
        {
            var p1 = viewer.ViewToMap(arr[i].x, arr[i].y);
            lst.Add(p1);
        }
        switch (optool.GetCurrentType())
        {
            case OperatingToolType.GISPoint:
                CreateFeature_point(lst[0]);
                break;
            case OperatingToolType.GISPolyline:
                CreateFeature_line(lst);
                break;
            case OperatingToolType.GISPolygon:
                CreateFeature_surface(lst);
                break;
            default:
                break;
        }
        Redraw();
    }

    public void Handle(string str, object par)
    {
        Type t = typeof(GisWrapper);
        object[] params_obj = new object[1];
        params_obj[0] = par;
        var m = t.GetMethod(str);
        var r = m.Invoke(this, params_obj);
    }
}
