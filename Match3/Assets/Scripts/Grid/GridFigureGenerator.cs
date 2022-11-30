using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridFigureGenerator : MonoBehaviour
{
    private int width, height;
    private float scale;
    private List<string> AvailableFiguresTypes = new List<string>();
    
    public void Setup(List<string> availableFiguresTypes, int width, int height, float scale)
    {
        this.width = width;
        this.height = height;
        this.scale = scale;
        AvailableFiguresTypes = availableFiguresTypes;
    }

    public TileObject NextRandomFigures(Transform parent)
    {
        var type = AvailableFiguresTypes[Random.Range(0, AvailableFiguresTypes.Count)];
        var figure = Instantiate(BaseResourcesSupplirs.PrefabsSupplier.GetObjectForID(type, "TileFigure"), parent).GetComponent<TileObject>();
        figure.transform.localScale = new Vector3(scale, scale, 1);
        return figure;
    }

    public List<List<TileObject>> InitalFigures(Transform parent)
    {
        List<List<TileObject>> map = new List<List<TileObject>>();

        for(int i = 0;i<width;i++)
        {
            map.Add(new List<TileObject>());
            for(int j = 0;j<height;j++)
            {
                map[i].Add(NextRandomFigures(parent));
            }
        }

        return map;

    }
}
