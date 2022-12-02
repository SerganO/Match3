using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager
{
    private static ScreenManager privateInstance = new ScreenManager();

    private ScreenManager() { }

    public static ScreenManager Shared
    {
        get
        {
            return privateInstance;
        }
    }

    int screenHeight
    {
        get
        {
            return Camera.main.pixelHeight;
        }
    }

    int screenWidth
    {
        get
        {
            return Camera.main.pixelWidth;
        }
    }

    public Vector3 WorldTopLeft()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0, 0));
    }

    public Vector3 WorldTopRight()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, 0));
    }

    public Vector3 WorldBottomLeft()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0, screenHeight));
    }

    public Vector3 WorldBottomRight()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight));
    }

    public float WorldWidth()
    {
        return WorldBottomRight().x - WorldBottomLeft().x;
    }

    public float WorldHeight()
    {
        return WorldBottomLeft().y - WorldTopLeft().y;
    }

    public Vector3 ScreenToWorldPoint(Vector3 point)
    {
        return Camera.main.ScreenToWorldPoint(point);
    }

    public Vector3 ScreenToWorldPoint(Vector2 point)
    {
        return Camera.main.ScreenToWorldPoint(point);
    }
    public float MapWidthScreenToWorld(float screenPoints)
    {
        return WorldWidth() / screenWidth * screenPoints;
    }

    public float MapHeightScreenToWorld(float screenPoints)
    {
        return WorldHeight() / screenHeight * screenPoints;
    }

    public float scaleForGrid(GridData gridData, float xOffset = 0, float yOffset = 0)
    {
        var width = screenWidth - xOffset;
        var height = screenHeight - yOffset;

        var elementWidth = width / gridData.Width;
        var elementHeight = height / gridData.Height;

        float xScale = MapWidthScreenToWorld(elementWidth);
        float yScale = MapHeightScreenToWorld(elementHeight);
        return Mathf.Min(xScale, yScale);
    }

    public enum PositionOrientation
    {
        Horizontal,
        Center,
        Vertical
    }

    public (PositionOrientation, float) ScaleOrientation(GridData gridData, RectTransform uiContainer)
    {
        var width = uiContainer.rect.width;
        var height = uiContainer.rect.height;

        var elementWidth = width / gridData.Width;
        var elementHeight = height / gridData.Height;

        float xScale = MapWidthScreenToWorld(elementWidth);
        float yScale = MapHeightScreenToWorld(elementHeight);
        if(xScale < yScale)
        {
            return (PositionOrientation.Horizontal, xScale / yScale);
        } else if(xScale > yScale)
        {
            return (PositionOrientation.Vertical, yScale / xScale);
        } else
        {
            return (PositionOrientation.Center, 1);
        }
    }


    public float scaleForGrid(GridData gridData, RectTransform uiContainer)
    {
        var width = uiContainer.rect.width;
        var height = uiContainer.rect.height;

        var elementWidth = width / gridData.Width;
        var elementHeight = height / gridData.Height;

        float xScale = MapWidthScreenToWorld(elementWidth);
        float yScale = MapHeightScreenToWorld(elementHeight);
        return Mathf.Min(xScale, yScale);
    }

}
