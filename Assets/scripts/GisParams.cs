using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;

public class GisParams {
    float resolution;
    Vector3 geo_center;
    Vector3 see_center;

    public void ResetResolution(Rect map, Rect view)
    {
        geo_center = map.center;
        see_center = view.center;
        float RoH = map.height / view.height;
        float RoW = map.width / view.width;
        resolution = RoH > RoW ? RoH : RoW;
    }

    public Vector3 ViewToMap(Vector3 pos)
    {
        Vector3 result = Vector3.zero;
        result.x = geo_center.x + (pos.x - see_center.x) * resolution;
        result.y = geo_center.y - (pos.y - see_center.y) * resolution;
        return result;
    }

    public Vector3 MapToView(Vector3 pos)
    {
        Vector3 result = Vector3.zero;
        result.x = see_center.x + ((pos.x - geo_center.x) / resolution + 0.5f);
        result.y = see_center.y - ((pos.y - geo_center.y) / resolution + 0.5f);
        return result;
    }
}
