using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[SelectionBase]
public class GridController : MonoBehaviour
{
    public bool turnInProcess = false;
    public event VoidFunc BuildCompleted;
    public GridFigureGenerator FigureGenerator;

    [Header("UI")]
    public GameObject Grid;
    public GameObject GridLine;
    public GridCell GridCell;

    public RectTransform UIContainer;

    [Header("Data")]
    public GridData GridData;

    public List<string> AvailableFiguresTypes = new List<string>();

    public List<List<TileObject>> figures = new List<List<TileObject>>();

    [Header("UI Data")]
    public float xOffset;
    public float yOffset;

    public float scale = 1;

    public GameObject GridObject;
    public List<List<GridCell>> Cells = new List<List<GridCell>>();

    private TileObject SelectedObject = null;


    public Transform FiguresParent;

    public GridCell BottomLeftCell
    {
        get
        {
            return Cells[0][0];
        }
    }

    public GridCell BottomRightCell
    {
        get
        {
            return Cells[0][GridData.Width - 1];
        }
    }

    public GridCell TopLeftCell
    {
        get
        {
            return Cells[GridData.Height - 1][0];
        }
    }

    public GridCell TopRightCell
    {
        get
        {
            return Cells[GridData.Height - 1][GridData.Width - 1];
        }
    }

    private void Start()
    {
        Build();
        InitFigures();
        Turn();
    }
    public void Build()
    {
        if(UIContainer != null)
        {
            float xDivider = 2;
            float yDivider = 2;

            var offsetData = ScreenManager.Shared.ScaleOrientation(GridData, UIContainer);

            switch (offsetData.Item1)
            {
                case ScreenManager.PositionOrientation.Horizontal:
                    yDivider /= offsetData.Item2;
                    break;
                case ScreenManager.PositionOrientation.Vertical:
                    xDivider /= offsetData.Item2;
                    break;
            }

            scale = ScreenManager.Shared.scaleForGrid(GridData, UIContainer);
            var gridObject = BuildFuncStep();
            var center = UIContainer.gameObject.transform.position;
            gridObject.transform.position = new Vector3(center.x - ScreenManager.Shared.MapWidthScreenToWorld(UIContainer.rect.width) / xDivider + scale / 2,
                center.y - ScreenManager.Shared.MapHeightScreenToWorld(UIContainer.rect.height) / yDivider + scale / 2);
            GridObject = gridObject;
        } else
        {
            scale = ScreenManager.Shared.scaleForGrid(GridData);
            var gridObject = BuildFuncStep();
            gridObject.transform.position = new Vector3(-((GridData.Width - 1) / 2.0f) * scale, -((GridData.Height - 1) / 2.0f) * scale);
            GridObject = gridObject;
        }
        BuildCompleted?.Invoke();
       
    }

    public GameObject BuildFuncStep()
    {
        transform.DestroyAllChilds();
        Cells = new List<List<GridCell>>();
        var gridObject = Instantiate(Grid, transform);
        for (int i = 0; i < GridData.Height; i++)
        {
            Cells.Add(new List<GridCell>());
            var lineObject = Instantiate(GridLine, gridObject.transform);
            for (int j = 0; j < GridData.Width; j++)
            {
                var cellObject = Instantiate(GridCell, lineObject.transform);
                cellObject.Data.Position.Set(j * scale, i * scale);
                cellObject.Data.IsEmpty = true;
                cellObject.transform.localScale = new Vector3(scale, scale, 1);
                cellObject.MoveToPosition();
                Cells[i].Add(cellObject);
            }
        }

        return gridObject;
    }

    public void InitFigures()
    {
        FigureGenerator.Setup(AvailableFiguresTypes, GridData.Width, GridData.Height, scale);
        var figures = FigureGenerator.InitalFigures(FiguresParent);
        this.figures = figures;
        for (int i = 0;i < GridData.Width;i++)
        {
            for (int j = 0; j < GridData.Height; j++)
            {
                MoveToIndexes(i, j);
                figures[i][j].TileData.x= i;
                figures[i][j].TileData.y= j;
            }
        }
       
    }


    public void Turn()
    {
        turnInProcess = true;
        if (Check())
        {

            Helper.Wait(this, 0.75f, () => {
                UpdateMap();
                Turn();
            });
        } else
        {
            UpdateMap();
            turnInProcess = false;
        }
        
    }
    public bool Check()
    {
        var res = false;
        for (int i = 0; i < GridData.Width; i++)
        {
            for (int j = 0; j < GridData.Height; j++)
            {
                var figure = figures[i][j];
                if (i > 0 && i < GridData.Width - 1)
                {
                    var left = figures[i - 1][j];
                    if (left.TileData.Type == figure.TileData.Type)
                    {
                        var right = figures[i + 1][j];
                        if (right.TileData.Type == figure.TileData.Type)
                        {
                            res = true;
                            left.TileData.IsForDelete = true;
                            figure.TileData.IsForDelete = true;
                            right.TileData.IsForDelete = true;
                        }

                    }
                }

                if (j > 0 && j < GridData.Height - 1)
                {
                    var top = figures[i][j - 1];
                    if (top.TileData.Type == figure.TileData.Type)
                    {
                        var bottom = figures[i][j + 1];
                        if (bottom.TileData.Type == figure.TileData.Type)
                        {
                            res = true;
                            top.TileData.IsForDelete = true;
                            figure.TileData.IsForDelete = true;
                            bottom.TileData.IsForDelete = true;
                        }

                    }
                }
            }
        }

        figures.ForEach(figList =>
        {
            figList.ForEach(fig =>
            {
                if(fig.TileData.IsForDelete)
                {
                    fig.Boom();
                }
            });
        });
        return res;
    }
    public void UpdateMap()
    {
        var addList = new List<int>();
        for (int i = 0; i < GridData.Width; i++)
            addList.Add(0);

        for (int i = GridData.Width - 1; i >= 0; i--)
        {
            for (int j = GridData.Height - 1; j >= 0; j--)
            {
                var fig = figures[i][j];

                if (fig.TileData.IsForDelete)
                {
                    for (int h = j - 1; h >= 0; h--)
                    {
                        if (!figures[i][h].TileData.IsForDelete && !figures[i][h].TileData.IsMovable)
                        {
                            figures[i][h].TileData.y = j;
                            figures[i][h].TileData.IsMovable = true;
                            break;
                        }
                    }
                    addList[i]++;
                }
            }
        }
        for (int i = GridData.Width - 1; i >= 0; i--)
        {
            for (int j = GridData.Height - 1; j >= 0; j--)
            {
                if (figures[i][j].TileData.IsForDelete)
                {
                   
                    Destroy(figures[i][j].gameObject);
                     figures[i].Remove(figures[i][j]);
                }
                    
            }
        }

        for (int i = 0; i < addList.Count; i++)
        {
            for(int j = 0; j < addList[i];j++)
            {
                var obj = FigureGenerator.NextRandomFigures(FiguresParent);
                figures[i].Insert(0, obj);
                figures[i][0].TileData.y = addList[i] - 1 - j;
                figures[i][0].TileData.x = i;
            }
            
        }

        

        for (int i = 0; i < GridData.Width; i++)
        {
            for (int j = 0; j < GridData.Height; j++)
            {
                MoveToIndexes(i, j);
                figures[i][j].TileData.x = i;
                figures[i][j].TileData.y = j;
                figures[i][j].TileData.IsMovable = false;
            }
        }


    }

    public void MoveToIndexes(int x, int y)
    {
        var startPoint = TopLeftCell.transform.position;
        figures[x][y].transform.position = new Vector3(startPoint.x + scale * x, startPoint.y - scale * y);
    }

    public void SwapFiguresLogic(int x1, int y1, int x2, int y2)
    {
        var temp = figures[x1][y1];
        figures[x1][y1] = figures[x2][y2];
        figures[x2][y2] = temp;
    }
    public void Swap(TileObject tile1, TileObject tile2, bool WithCheck = true)
    {
        turnInProcess = true;
        SwapFiguresLogic(tile1.TileData.x, tile1.TileData.y, tile2.TileData.x, tile2.TileData.y);
        MoveToIndexes(tile1.TileData.x, tile1.TileData.y);
        MoveToIndexes(tile2.TileData.x, tile2.TileData.y);

        tile1.Deselect();
        tile2.Deselect();
        if (WithCheck)
        {
            Helper.Wait(this, 0.15f, () =>
            {
                if (Check())
                {
                    Turn();
                }
                else
                {
                    SelectedObject = null;
                    Swap(tile1, tile2, false);
                    turnInProcess = false;
                }
            });
        } else {
            turnInProcess = false;
        }


    }


    public void Select(TileObject tile)
    {
        if(SelectedObject == null)
        {
            SelectedObject= tile;
        } else
        {
            if(SelectedObject.TileData.x == tile.TileData.x)
            {
                if(SelectedObject.TileData.y == tile.TileData.y - 1 || SelectedObject.TileData.y == tile.TileData.y + 1)
                {
                    Swap(SelectedObject, tile);
                }
            }
            else if (SelectedObject.TileData.y == tile.TileData.y)
            {
                if (SelectedObject.TileData.x == tile.TileData.x - 1 || SelectedObject.TileData.x == tile.TileData.x + 1)
                {
                    Swap(SelectedObject, tile);
                }
            } else
            {
                SelectedObject.Deselect();
                SelectedObject = tile;
            }
        }
    }

    public void Deselect(TileObject tile)
    {
        SelectedObject = null;
    }
}