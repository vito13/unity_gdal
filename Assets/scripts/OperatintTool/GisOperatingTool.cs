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

