using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSGeo.OGR;
using DigitalRuby.FastLineRenderer;

public class SpeedRoad : MonoBehaviour
{
    static public GameObject prefab = null;
    SpeedRoadSectionMgr sectionMgr = new SpeedRoadSectionMgr();
    SpeedRoadCrossingMgr crossingMgr = new SpeedRoadCrossingMgr();
    PathFinder pf = new PathFinder();

    [SerializeField]
    float wayWidth;
    public static float RoadwayWidth; // 单条车道宽度
    [SerializeField]
    float angleThreshold;
    public static float RoadAngleThreshold; // 用于优化路段中比较连续两点之间的角度差
    [SerializeField]
    float zebraCrossingLength; // 人行道长度
    public static float RoadZebraCrossingLength;
    [SerializeField]
    int zebraCrossings4Way; // 一条车道有几条斑马线
    public static int RoadZebraCrossings4Way;
    [SerializeField]
    float waitingLength; // 候车箭头部分长度
    public static float RoadWaitingLength;
    [SerializeField]
    int textureHeightPreWay; // 一条车道的纹理高度
    public static int RoadTextureHeightPreWay;
    [SerializeField]
    int pathStart;
    [SerializeField]
    int pathEnd;
    [SerializeField]
    float duration;

    [SerializeField]
    TrafficSporter sporter;

    // Use this for initialization
    void Start()
    {
        Ogr.RegisterAll();
        RoadwayWidth = wayWidth;
        RoadAngleThreshold = angleThreshold;
        RoadZebraCrossingLength = zebraCrossingLength;
        RoadZebraCrossings4Way = zebraCrossings4Way;
        RoadWaitingLength = waitingLength;
        RoadTextureHeightPreWay = textureHeightPreWay;

        prefab = Resources.Load("empty") as GameObject;
        SpeedRoadTexMgr.Instance.Init();
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
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            LoadFile();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            FindPath();
        }
    }

    void LoadFile()
    {

        {
            string fname = "E:\\test\\---\\after_sections.shp";
            GameObject sectionMgrObj = Instantiate(prefab);
            sectionMgrObj.transform.parent = transform;
            sectionMgrObj.name = "SpeedRoadSectionMgr";
            sectionMgr.LoadFile(fname, sectionMgrObj.transform);
        }

        {
            string fname = "E:\\test\\---\\after_crossings.shp";
            GameObject crossingMgrObj = Instantiate(prefab);
            crossingMgrObj.transform.parent = transform;
            crossingMgrObj.name = "SpeedRoadCrossingMgr";
            crossingMgr.LoadFile(fname, crossingMgrObj.transform, sectionMgr);
        }

        var graph = crossingMgr.BuildGraph(sectionMgr);
        pf.Init(graph);
    }

    void FindPath()
    {
        List<Vector3> path = new List<Vector3>();
        var r = pf.FindShortestPath(pathStart, pathEnd, new List<long>());
        print(r.Count);
        if (r.Count > 0)
        {
            foreach (var item in r)
            {
                print(item);
            }
            path = pf.GetPathPoint(r, crossingMgr);
            
//             for (int i = 0; i < path.Count; i++)
//             {
//                 GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//                 sphere.transform.parent = transform;
//                 sphere.transform.localPosition = path[i];
//             }
            sporter.Init(path, duration);
        }
    }
}
