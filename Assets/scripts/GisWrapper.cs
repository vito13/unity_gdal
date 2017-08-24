using DigitalRuby.FastLineRenderer;
using OSGeo.OGR;
using UnityEngine;

public class GisWrapper{
    GisViewer viewer = null;
    GisModel model = null;
    GisRenderer fastrenderer = null;


    public void Init (FastLineRenderer renderer) {
        Ogr.RegisterAll();
        model = new GisModel();
        viewer = new GisViewer();
        viewer.Init();
        fastrenderer = new GisRenderer();
        fastrenderer.Init(renderer, viewer);
    }
	

    public void LoadFile(string fname)
    {
        model.Open(fname);
    }

    public void FullExtent()
    {
        Rect env = Rect.zero;
        model.GetEnvelpoe(ref env);
        Rect clientSize = Rect.zero;
        clientSize.width = Screen.width;
        clientSize.height = Screen.height;
        viewer.ResetResolution(env, clientSize);
        viewer.Init();
    }

    public void Translation(Vector3 offset)
    {
        viewer.Translate(offset);
    }

    public void Zooming(Vector3 center, float val)
    {
        viewer.Zooming(center, val);
    }

    public void Redraw()
    {
        fastrenderer.Clear();
        model.ResetFeatureIterator();
        fastrenderer.BeginDraw();
        string json = "";
        while ((json = model.GetNextFeatureGeometryJson()).Length > 0)
        {
            fastrenderer.DrawGeometry(json);
        }
        fastrenderer.EndDraw();
    }

    public Vector3 MapToView(Vector3 pos)
    {
        return viewer.MapToView(pos);
    }

    public Vector3 ViewToMap(Vector3 pos)
    {
        return viewer.ViewToMap(pos);
    }

    public string GetStateInfo()
    {
        Rect env = Rect.zero;
        model.GetEnvelpoe(ref env);
        string result = string.Format("窗口坐标: {0}; 地图坐标: {1}\n窗口中心点: {2}对应的地图坐标: {3}\n窗口尺寸: ({4}, {5}); 地图范围: {6}\n比例尺: {7}",
            Input.mousePosition.ToString(), ViewToMap(Input.mousePosition).ToString(),
            viewer.GetSeeCenter().ToString(), ViewToMap(viewer.GetSeeCenter()).ToString(),
            Screen.width, Screen.height, env.ToString(),
            viewer.GetResolution()
            );
        return result;
    }
}
