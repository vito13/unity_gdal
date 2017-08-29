using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;
using System.Collections.Generic;
using UnityEngine;

public class GisWrapper{
    GisViewer viewer = null;
    GisModel model = null;
    GisRenderer fastrenderer = null;
    GisSelectionSet ss = null;


    public void Init (FastLineRenderer defaultRenderer, FastLineRenderer selectionRenderer) {
        Ogr.RegisterAll();
        model = new GisModel();
        viewer = new GisViewer();
        viewer.Init();
        fastrenderer = new GisRenderer();
        fastrenderer.Init(defaultRenderer, 0.02f);

        ss = new GisSelectionSet();
        ss.Init(selectionRenderer, viewer);
    }
	

    public void LoadFile(string fname)
    {
        model.Open(fname);
    }

    public void FullExtent()
    {
        var env = model.GetEnvelpoe();
        Rect clientSize = Rect.zero;
        clientSize.width = Screen.width;
        clientSize.height = Screen.height;
        viewer.ResetResolution(utils.ECEnvlope2Rect(env), clientSize);
        viewer.Init();
        model.SetSpatialFilterRect(env);
    }

    public void Translation(Vector3 offset)
    {
        if (utils.PositionInView(Input.mousePosition) && offset != Vector3.zero)
        {
            viewer.Translate(offset);
        }
    }

    public void Zooming(Vector3 scaleCenter, float val)
    {
        if (utils.PositionInView(scaleCenter) && val != 0) 
        {
            viewer.Zooming(scaleCenter, val > 0 ? true : false);
        }
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
            TransformGeometry2View(ref geo);
            fastrenderer.DrawGeometry(geo);
            geo.Dispose();
        }
        fastrenderer.EndDraw();
    }

    void TransformGeometry2View(ref Geometry geo)
    {
        wkbGeometryType t = Ogr.GT_Flatten(geo.GetGeometryType());
        switch (t)
        {
            case wkbGeometryType.wkbUnknown:
                break;
            case wkbGeometryType.wkbPoint:
                break;
            case wkbGeometryType.wkbPolygon:
                {
                    Geometry linestring = geo.GetGeometryRef(0);
                    if(Ogr.GT_Flatten(linestring.GetGeometryType()) == wkbGeometryType.wkbLineString)
                    {
                        TransformGeometry2View(ref linestring);
                    }
                }
                break;
            case wkbGeometryType.wkbLineString:
                {
                    var count = geo.GetPointCount();
                    double[] pt = new double[2];
                    for (int i = 0; i < count; i++)
                    {
                        geo.GetPoint_2D(i, pt);
                        var viewpt = MapToView(pt[0], pt[1]);
                        geo.SetPoint_2D(i, viewpt.x, viewpt.y);
                    }
                }
                break;
            default:
                break;
        }
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
            "窗口坐标: {0}; 地图坐标: {1}" +
            "\n窗口中心点: {2}对应的地图坐标: {3}" +
            "\n窗口尺寸: ({4}, {5}); 地图范围: {6}" +
            "\n比例尺: {7}; 摄像机Size: {8}" +
            "\n当前呈现地图范围： {9}\n摄像机位置: {10}; 范围: (xmin: {11}, xmax: {12}, ymin: {13}, ymax: {14})",
            Input.mousePosition.ToString(), ViewToMap(Input.mousePosition.x, Input.mousePosition.y).ToString(),
            viewer.GetSeeCenter().ToString(), ViewToMap(viewer.GetSeeCenter().x, viewer.GetSeeCenter().y).ToString(),
            Screen.width, Screen.height, env.ToString(),
            viewer.GetResolution(), Camera.main.orthographicSize,
            viewer.GetCurrentMapRect().ToString(),
            Camera.main.transform.position.ToString(),
            Camera.main.transform.position.x - w * 0.5f,
            Camera.main.transform.position.x + w * 0.5f,
            Camera.main.transform.position.y - h * 0.5f,
            Camera.main.transform.position.y + h * 0.5f
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
            ss.Add(item.fea);
        }
        ss.Redraw();

        return lst;
    }
}
