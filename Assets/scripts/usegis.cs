using DigitalRuby.FastLineRenderer;
using UnityEngine;
using UnityEngine.UI;

public class usegis : MonoBehaviour
{
    [SerializeField]
    FastLineRenderer lineRenderer = null;
    [SerializeField]
    Text stateinfo;

    GisWrapper gis = null;
    Vector3 mouseDown = Vector3.zero;

    void Awake()
    {
        gis = new GisWrapper();
        gis.Init(lineRenderer);
        gis.LoadFile("D:\\000testdata\\hgd\\line.shp");
        gis.FullExtent();
        gis.Redraw();
    }

    void Update()
    {
        Offseting();
        Zooming();
        stateinfo.text = gis.GetStateInfo();
    }

    void Zooming()
    {
        var sw = Input.GetAxis("Mouse ScrollWheel");
        if (sw != 0)
        {
            gis.Zooming(Input.mousePosition, sw);
        }
    }

    void Offseting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            var dis = mouseDown - Input.mousePosition;
            gis.Translation(dis);
            mouseDown = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            mouseDown = Vector3.zero;
        }
    }

    public void FullExtend()
    {
        gis.FullExtent();
    }
   
}
