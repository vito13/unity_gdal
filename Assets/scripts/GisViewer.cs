using OSGeo.OGR;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using UnityEngine.Assertions;

public class GisViewer
{
    double ratioZooming = 1.1f;
    private double resolution;
    private CPPOGREnvelope view, map;
    double standardrate = 1;

    public void ResetStandardRate()
    {
        standardrate = resolution;
    }

    public void Init()
    {
        Assert.IsNotNull(Camera.main);
        Camera.main.orthographic = true;
        Camera.main.farClipPlane = 1000;
        Camera.main.nearClipPlane = 0;
        Camera.main.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -100);
        Camera.main.orthographicSize = Screen.height / 2;
    }

    public Vector2D GetSeeCenter()
    {
        return view.GetCenter();
    }
    public Vector2D GetGeoCenter()
    {
        return map.GetCenter();
    }
    public double GetResolution()
    {
        return resolution;
    }
    public CPPOGREnvelope GetCurrentMapRect()
    {
        return map;
    }

    public void ResetResolution(CPPOGREnvelope m, CPPOGREnvelope v)
    {
        view = v;
        map = m;
        double RoH = map.GetHeight() / view.GetHeight();
        double RoW = map.GetWidth() / view.GetWidth();
        resolution = RoH > RoW ? RoH : RoW;
    }

    public Vector2D ViewToMap(double x, double y)
    {
        var mc = map.GetCenter();
        var vc = view.GetCenter();
        Vector2D r = new Vector2D(mc.x + (x - vc.x) * resolution, mc.y + (y - vc.y) * resolution);
        return r;
    }

    public Vector2D MapToView(double x, double y)
    {
        var mc = map.GetCenter();
        var vc = view.GetCenter();
        Vector2D result = new Vector2D(
            vc.x + ((x - mc.x) / resolution),
            vc.y + ((y - mc.y) / resolution));

        var r = ViewToCamera(result);
        return r; // result;
    }

    Vector2D ViewToCamera(Vector2D pt)
    {
        var envcam = utils.GetOrthographicCameraEnvelope();
        var envview = utils.GetViewerEnvelope();
        var x = pt.x / envview.GetWidth() * envcam.GetWidth() + envcam.MinX;
        var y = pt.y / envview.GetHeight() * envcam.GetHeight() + envcam.MinY;
        return new Vector2D(x, y);
    }

    /// <summary>
    /// 平移view的视野中心，会改变map的env与摄像机的位置
    /// </summary>
    /// <param name="offset"></param>
    public void Translate(Vector2D offset)
    {
        if (!utils.PositionInView(Input.mousePosition))
        {
            return;
        }
        // map是根据当前比例平移即可
        map.Translate(new Vector2D(offset.x * resolution, offset.y * resolution));
        /* 摄像机平移则需要一个比例，此比例是当前“scale/全景时的scale”
         * 此数也是根据不错测试推测而得。因为全景时候map与摄像机平移相同，但比例不一样时则不同，
         * 所以推测应该以全景做参照
         */
        Camera.main.transform.Translate(new Vector3(
            (float)(offset.x * resolution / standardrate),
            (float)(offset.y * resolution / standardrate),
            0));
    }

    public void Zooming(Vector3 scaleCenter, bool zoomin)
    {
        if (!utils.PositionInView(scaleCenter))
        {
            return;
        }
        Vector2D oldwh = utils.GetCameraWH();
        float x = 0;
        float y = 0;
        if (zoomin)
        {
            // 计算wh的变化量
            var wh = new Vector2D(map.GetWidth() - map.GetWidth() / ratioZooming, map.GetHeight() - map.GetHeight() / ratioZooming);
            // 计算缩放点在视口中的百分比
            var x1 = Input.mousePosition.x / Screen.width;
            // 根据百分比对各边进行增减
            map.MinX += wh.x * x1;
            map.MaxX -= wh.x * (1.0f - x1);
            var y1 = Input.mousePosition.y / Screen.height;
            map.MinY += wh.y * y1;
            map.MaxY -= wh.y * (1.0f - y1);
            // 对摄像机的size进行scale
            Camera.main.orthographicSize /= (float)ratioZooming;
            // 记录xy百分比用于摄像机的平移，上面对map的改变已包含缩放中心效果
            x = x1;
            y = y1;
        }
        else
        {
            var wh = new Vector2D(map.GetWidth() * ratioZooming - map.GetWidth(), map.GetHeight() * ratioZooming - map.GetHeight());
            var x1 = Input.mousePosition.x / Screen.width;
            map.MinX -= wh.x * x1;
            map.MaxX += wh.x * (1.0f - x1);
            var y1 = Input.mousePosition.y / Screen.height;
            map.MinY -= wh.y * y1;
            map.MaxY += wh.y * (1.0f - y1);
            Camera.main.orthographicSize *= (float)ratioZooming;

            x = x1;
            y = y1;
        }
        ResetResolution(map, view);
        ResetCameraPos(oldwh, x, y);
    }

    void ResetCameraPos(Vector2D oldwh, float x1, float y1)
    {
        Vector2D newwh = utils.GetCameraWH();
        var disx = newwh.x - oldwh.x;
        var disy = newwh.y - oldwh.y;
        /* 
         * 取缩放前后的wh差距，即为需要移动的距离（其实应为移动距离的两倍，就像是下面内框移动的范围其实就是两框wh差距的一半）
           前面对摄像机的size进行scale但位置并未改变，仅仅是视野矩形缩放了，
         -------------------
        |    -----------    |
        |   |           |   |
        |   |           |   |
        |    -----------    |
         -------------------
         下面的0.5就是以摄像机位置为原点，X1范围0～1，所以最终可以得到向左或向右的距离
        */
        Camera.main.transform.Translate(new Vector3(
            (0.5f - x1) * (float)disx,
            (0.5f - y1) * (float)disy,
            0
            ));
    }

    public void TransformGeometry2View(ref Geometry geo)
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
                    if (Ogr.GT_Flatten(linestring.GetGeometryType()) == wkbGeometryType.wkbLineString)
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
}
