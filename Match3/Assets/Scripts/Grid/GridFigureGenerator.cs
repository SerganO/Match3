using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

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
        return NextRandomFigures(AvailableFiguresTypes, parent);
    }

    public TileObject NextRandomFigures(List<string> availableFiguresTypes, Transform parent)
    {
        var type = availableFiguresTypes[UnityEngine.Random.Range(0, availableFiguresTypes.Count)];
        var figure = Instantiate(BaseResourcesSuppliers.PrefabsSupplier.GetObjectForID(type, "TileFigure"), parent).GetComponent<TileObject>();
        figure.transform.localScale = new Vector3(scale, scale, 1);
        return figure;
    }

    public TileObject FigureForTypeString(string type, Transform parent)
    {
        var figure = Instantiate(BaseResourcesSuppliers.PrefabsSupplier.GetObjectForID(type, "TileFigure"), parent).GetComponent<TileObject>();
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

    public List<List<TileObject>> AutomergelessInitalFigures(Transform parent)
    {
        List<List<TileObject>> map = new List<List<TileObject>>();

        var firstFiguresSet = new List<string>();
        var secondFiguresSet = new List<string>();

        for(int i = 0;i < AvailableFiguresTypes.Count;i++)
        {
            if(i%2==0)
            {
                secondFiguresSet.Add(AvailableFiguresTypes[i]);
            } else
            {
                firstFiguresSet.Add(AvailableFiguresTypes[i]);
            }
        }

        for (int i = 0; i < width; i++)
        {
            map.Add(new List<TileObject>());
            for (int j = 0; j < height; j++)
            {
                if((i + j) % 2 == 0)
                {
                    map[i].Add(NextRandomFigures(secondFiguresSet, parent));
                } else
                {
                    map[i].Add(NextRandomFigures(firstFiguresSet, parent));
                }
                
            }
        }

        return map;

    }

    public List<List<TileObject>> AlternativeAutomergelessInitalFigures(Transform parent, bool withGurantineTurn = true)
    {
        List<List<TileObject>> map = new List<List<TileObject>>();
        var random = new System.Random();
        var result = AvailableFiguresTypes.OrderBy(item => random.Next()).ToList();
        var firstFiguresSet = new List<string>();
        var secondFiguresSet = new List<string>();

        for (int i = 0; i < result.Count; i++)
        {
            if (i % 2 == 0)
            {
                firstFiguresSet.Add(result[i]);
            }
            else
            {
                secondFiguresSet.Add(result[i]);
            }
        }

        var pattern = new List<List<string>>{
            new List<string> { "o","o","*", "*" },
            new List<string> { "o","*","o", "*" },
            new List<string> { "*","*","o", "o" },
            new List<string> { "*","0","*", "o" },
         };
        int patternHeight = pattern.Count;
        var debugImage = "";
        if(patternHeight == 0)
        {
            return InitalFigures(parent);
        }
        int patternWidth = pattern[0].Count;
        if(patternWidth == 0)
        {
            return InitalFigures(parent);
        }
        

        for (int i = 0; i < width; i++)
        {
            map.Add(new List<TileObject>());
            for (int j = 0; j < height; j++)
            {
                if (withGurantineTurn)
                {
                    if ((i == 0 && (j == 0 || j == 1)) || (j == 2 && i == 1))
                    {
                        map[i].Add(FigureForTypeString(firstFiguresSet[0], parent));
                        debugImage += "G";
                        continue;
                    }



                }
                    int actualI = i % patternWidth;
                int actualJ = j % patternHeight;

                var sign = pattern[actualI][actualJ];
                debugImage += sign;
                if (sign == "*")
                {
                    map[i].Add(NextRandomFigures(secondFiguresSet, parent));
                }
                else
                {

                    map[i].Add(NextRandomFigures(firstFiguresSet, parent));
                }

            }
            debugImage += Environment.NewLine;
        }
        Debug.Log(debugImage);
        return map;

    }
}
