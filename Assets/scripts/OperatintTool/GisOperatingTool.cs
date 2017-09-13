using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

public enum OperatingToolType
{
    GISUnknow = -1,
    GISHand = 0,
    GISPoint,
    GISPolyline,
    GISPolygon,
    GISBoxSelect,
    GISPointSelect,
    GISCircleSelect,
    GISPolygonSelect,
}

public enum SpatialRelation_TYPE
{
    hhhwSRT_UNKNOWN = -1,
    hhhwSRT_Within = 0, // 几何体在矩形内
    hhhwSRT_Intersect, // 几何体与矩形有交叉
    hhhwSRT_COUNT,
}

public interface IMouseAction
{
    void OnButtonDown();
    void OnButton();
    void OnButtonUp();
    void OnWheel(bool t);
    void OnMove();
    void OnDblClk();
}


public class GisOperatingTool : IMouseAction
{
    protected OperatingToolType type;
    protected Vector2 originalPos;
    protected VectorLine line;
    static protected GisOperatingToolSet host;

    public static void SetHost(GisOperatingToolSet h)
    {
        host = h;
    }

    public void Send(string str, object par)
    {
        host.Send(str, par);
    }

    public GisOperatingTool()
    {
        init();
    }

    public OperatingToolType GetToolType()
    {
        return type;
    }

    public virtual void OnButtonDown()
    {
    }

    public virtual void OnButton()
    {

    }

    public virtual void OnButtonUp()
    {

    }

    public virtual void OnWheel(bool t)
    {

    }

    public virtual void OnMove()
    {

    }
    public virtual void OnDblClk()
    {
    }

    public virtual void Reset()
    {
    }

    protected virtual void init()
    {

    }

}

