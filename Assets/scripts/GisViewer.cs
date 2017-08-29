using UnityEngine;


public class GisViewer
{
    float ratioZooming = 1.2f;
    private double resolution;
    private Rect view, map;

    public void Init()
    {
        Camera.main.orthographic = true;
        Camera.main.farClipPlane = 1000;
        Camera.main.nearClipPlane = 0;
        Camera.main.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -100);
        Camera.main.orthographicSize = Screen.height / 2;
    }

    public Vector3 GetSeeCenter()
    {
        return view.center;
    }
    public Vector3 GetGeoCenter()
    {
        return map.center;
    }
    public double GetResolution()
    {
        return resolution;
    }
    public Rect GetCurrentMapRect()
    {
        return map;
    }

    public void ResetResolution(Rect m, Rect v)
    {
        view = v;
        map = m;
        double RoH = map.height / view.height;
        double RoW = map.width / view.width;
        resolution = RoH > RoW ? RoH : RoW;
    }

    public Vector2D ViewToMap(float x, float y)
    {
        Vector2D r = new Vector2D();
        r.x = map.center.x + (x - view.center.x) * resolution;
        r.y = map.center.y + (y - view.center.y) * resolution;
        return r;
    }

    public Vector2D MapToView(double x, double y)
    {
        Vector2D result = new Vector2D();
        result.x = view.center.x + ((x - map.center.x) / resolution + 0.5f);
        result.y = view.center.y + ((y - map.center.y) / resolution + 0.5f);
        return result;
    }

    public void Translate(Vector3 offset)
    {
        //         float rate = firstSize / Camera.main.orthographicSize;
        //         offset /= rate;
        // 
        return;
 //       Camera.main.transform.Translate(offset);
//         geo_center = ViewToMap(see_center + offset);
    }


    public void Zooming(Vector3 scaleCenter, bool zoomin)
    {
        /*
        var scaleCenterMap = ViewToMap(scaleCenter);
        var cen2xmin = scaleCenterMap.x - map.xMin;
        var cen2xmax = map.xMax - scaleCenterMap.x;

        var cen2ymin = scaleCenterMap.y - map.yMin;
        var cen2ymax = map.yMax - scaleCenterMap.y;

        float size = 0;
        if (zoomin)
        {
            map.xMin += cen2xmin / ratioZooming;
            map.xMax -= cen2xmax / ratioZooming;

            map.yMin += cen2ymin / ratioZooming;
            map.yMax -= cen2ymax / ratioZooming;
            size = Camera.main.orthographicSize / ratioZooming;
        }
        else
        {
            map.xMin -= cen2xmin * ratioZooming;
            map.xMax += cen2xmax * ratioZooming;

            map.yMin -= cen2ymin * ratioZooming;
            map.yMax += cen2ymax * ratioZooming;
            size = Camera.main.orthographicSize * ratioZooming;
        }

        if (size != 0)
        {
            Camera.main.orthographicSize = size;
            ResetResolution(map, view);
        }
       

        var cx = scaleCenter.x - view.center.x;
        var cy = scaleCenter.y - view.center.y;
      //  Camera.main.transform.Translate(new Vector3(cx / ratioZooming, cy / ratioZooming, 0));
      */
    }
}
