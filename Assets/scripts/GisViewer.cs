using UnityEngine;


public class GisViewer
{
    float ratioZooming = 1.3f;

    private float resolution;
    private Vector3 geo_center;
    private Vector3 see_center;

    
    public void Init()
    {
        Camera.main.orthographic = true;
        Camera.main.farClipPlane = 1000;
        Camera.main.nearClipPlane = 0;
        Camera.main.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -100);
        Camera.main.orthographicSize = Screen.height / 2;
    }

    public void Zooming(Vector3 center, float val)
    {
        //         var sw = Input.GetAxis("Mouse ScrollWheel");
        //         float dis = 0;
        //        
        //         if (sw < 0)
        //         {
        //             var bak = Camera.main.orthographicSize;
        //             Camera.main.orthographicSize *= ratioZooming;
        //             dis = Camera.main.orthographicSize - bak;
        //         }
        //         else if (sw > 0)
        //         {
        //             var bak = Camera.main.orthographicSize;
        //             Camera.main.orthographicSize /= ratioZooming;
        //             dis = Camera.main.orthographicSize - bak;
        //         }
        // 
        //         if (dis != 0)
        //         {
        // 
        //         }
    }


   


    public Vector3 GetSeeCenter()
    {
        return see_center;
    }
    public Vector3 GetGeoCenter()
    {
        return geo_center;
    }
    public float GetResolution()
    {
        return resolution;
    }


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
        result.y = geo_center.y + (pos.y - see_center.y) * resolution;
        return result;
    }

    public Vector3 MapToView(Vector3 pos)
    {
        Vector3 result = Vector3.zero;
        result.x = see_center.x + ((pos.x - geo_center.x) / resolution + 0.5f);
        result.y = see_center.y + ((pos.y - geo_center.y) / resolution + 0.5f);
        return result;
    }

    public void Translate(Vector3 offset)
    {
        Camera.main.transform.Translate(offset);
        geo_center = ViewToMap(see_center + offset);
    }
}
