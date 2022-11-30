using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridCell : MonoBehaviour
{
    public GridCellData Data = new GridCellData();
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public void MoveToPosition()
    {
        transform.position = new Vector3(Data.Position.x, Data.Position.y);
    }

    public void ClearPlaceholder()
    {
        sr.color = Color.white;
    }
    public void ShowPlaceholder(bool value)
    {
        sr.color = value ? Color.green : Color.red;
    }
}

[System.Serializable]
public class GridCellData
{
    public Vector2 Position = new Vector2();
    public bool IsEmpty;
    public string elementId;
}
