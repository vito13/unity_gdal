using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;

public class SpeedRoad : MonoBehaviour {
    GameObject prefab = null;
    SpeedRoadSectionMgr sectionMgr = new SpeedRoadSectionMgr();
    SpeedRoadCrossingMgr crossingMgr = new SpeedRoadCrossingMgr();
    [SerializeField]
    float wayWidth;
    public static float RoadwayWidth;
    [SerializeField]
    float angleThreshold;
    public static float AngleThreshold;

    // Use this for initialization
    void Start () {
        Ogr.RegisterAll();
        RoadwayWidth = wayWidth;
        AngleThreshold = angleThreshold;
        prefab = Resources.Load("empty") as GameObject;

         /*

        Geometry pt = new Geometry(wkbGeometryType.wkbLineString);
        pt.AddPoint_2D(0, 0);
        pt.AddPoint_2D(10, 10);
        pt.AddPoint_2D(0, 10);
        print(pt.Length());

        Vector3 v1 = new Vector3((float)pt.GetX(0), (float)pt.GetY(0), 0);
        Vector3 v2 = new Vector3((float)pt.GetX(1), (float)pt.GetY(1), 0);
        float d = Vector3.Distance(v2, v1);
        print(d);
        var r = Vector3.Lerp(v1, v2, (d - 2) / d);
        print(r);
 */
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.A))
        {
            LoadFile();
        }
	}

    void LoadFile() {

        {
            string fname = "E:\\test\\---\\after_sections.shp";
            GameObject sectionMgrObj = Instantiate(prefab);
            sectionMgrObj.transform.parent = transform;
            sectionMgrObj.name = "SpeedRoadSectionMgr";
            sectionMgr.LoadFile(fname, sectionMgrObj.transform, prefab);
        }

        {
            string fname = "E:\\test\\---\\after_crossings.shp";
            GameObject crossingMgrObj = Instantiate(prefab);
            crossingMgrObj.transform.parent = transform;
            crossingMgrObj.name = "SpeedRoadCrossingMgr";
            crossingMgr.LoadFile(fname, crossingMgrObj.transform, prefab);
            crossingMgr.CalculateCrossingMesh(sectionMgr);
        }
    }
}
