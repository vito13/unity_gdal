using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GisOperatingToolSet {
    Dictionary<OperatingToolType, GisOperatingTool> mapTools = null;
    GisOperatingTool currentTool = null;
    GisViewer viewer = null;

    public void Init(GisViewer v)
    {
        viewer = v;
        mapTools = new Dictionary<OperatingToolType, GisOperatingTool>();
        mapTools.Add(OperatingToolType.GISHand, new GisHandTool(viewer));
        mapTools.Add(OperatingToolType.GISPolyline, new GisPolylineTool());
        mapTools.Add(OperatingToolType.GISPolygon, new GisPolygonTool());
        mapTools.Add(OperatingToolType.GISPoint, new GisPointTool());
        SetCurrentType(OperatingToolType.GISHand);
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
