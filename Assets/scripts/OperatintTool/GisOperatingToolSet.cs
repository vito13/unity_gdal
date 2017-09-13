using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GisOperatingToolSet {
    Dictionary<OperatingToolType, GisOperatingTool> mapTools = null;
    GisOperatingTool currentTool = null;
    GisViewer viewer = null;
    static GisWrapper host;
    public static SpatialRelation_TYPE sqtype;

    public static void SetHost(GisWrapper h)
    {
        host = h;
    }

    public void Send(string str, object par)
    {
        host.Handle(str, par);
    }

    public void Init(GisViewer v)
    {
        viewer = v;
        GisOperatingTool.SetHost(this);
        mapTools = new Dictionary<OperatingToolType, GisOperatingTool>();
        mapTools.Add(OperatingToolType.GISHand, new GisHandTool(viewer));
        mapTools.Add(OperatingToolType.GISPolyline, new GisPolylineTool());
        mapTools.Add(OperatingToolType.GISPolygon, new GisPolygonTool());
        mapTools.Add(OperatingToolType.GISPoint, new GisPointTool());
        mapTools.Add(OperatingToolType.GISBoxSelect, new GisBoxSelect());
        mapTools.Add(OperatingToolType.GISPointSelect, new GisPointSelect());
        mapTools.Add(OperatingToolType.GISCircleSelect, new GisCircleSelect());
        mapTools.Add(OperatingToolType.GISPolygonSelect, new GisPolygonSelect());
        SetCurrentType(OperatingToolType.GISHand);
        sqtype = SpatialRelation_TYPE.hhhwSRT_Intersect;
    }

    public GisOperatingTool GetCurrentTool()
    {
        return currentTool;
    }

    public OperatingToolType GetCurrentType()
    {
        return currentTool.GetToolType();
    }

    public bool SetCurrentType(OperatingToolType t)
    {
        bool r = false;
        if (mapTools.ContainsKey(t))
        {
            currentTool = mapTools[t];
            r = true;
        }
        return r;
    }

    public void OnButtonDown()
    {
        GetCurrentTool().OnButtonDown();
    }

    public void OnButton()
    {
        GetCurrentTool().OnButton();
    }

    public void OnButtonUp()
    {
        GetCurrentTool().OnButtonUp();
    }

    public void OnWheel(bool t)
    {
        GetCurrentTool().OnWheel(t);
    }

    public void OnMove()
    {
        GetCurrentTool().OnMove();
    }

    public void OnDblClk()
    {
        GetCurrentTool().OnDblClk();
    }

}
