using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public string Type;
    public bool IsMovable;
    public bool IsForDelete;

    public int x;
    public int y;
}
