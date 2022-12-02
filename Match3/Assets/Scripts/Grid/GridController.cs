using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[SelectionBase]
public class GridController : MonoBehaviour
{
    public bool turnInProcess = false;
    public event VoidFunc BuildCompleted;
    public GridFigureGenerator FigureGenerator;
    public GestureManager GestureManager;

    [Header("UI")]
    public GameObject Grid;
    public GameObject GridLine;
    public GridCell GridCell;

    public RectTransform MessageContainer;

    public RectTransform UIContainer;

    [Header("Data")]
    public GridData GridData;

    int gridWidth
    {
        get
        {
            return GridData.Width;
        }
    }

    int gridHeight
    {
        get
        {
            return GridData.Height;
        }
    }

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
        Rebuild();
        Bind();
    }

    public void Rebuild()
    {
        Build();
        InitFigures();
        Turn();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    void Bind()
    {
        GestureManager.UpSwipe += GestureManager_UpSwipe;
        GestureManager.DownSwipe += GestureManager_DownSwipe;
        GestureManager.LeftSwipe += GestureManager_LeftSwipe;
        GestureManager.RightSwipe += GestureManager_RightSwipe;
    }

    private void GestureManager_UpSwipe()
    {
        SwipeHandle(DraggedDirection.Up);
    }
     private void GestureManager_DownSwipe()
    {
        SwipeHandle(DraggedDirection.Down);
    }
     private void GestureManager_LeftSwipe()
    {
        SwipeHandle(DraggedDirection.Left);
    }
     private void GestureManager_RightSwipe()
    {
        SwipeHandle(DraggedDirection.Right);
    }

    void Unbind()
    {
        GestureManager.UpSwipe -= GestureManager_UpSwipe;
        GestureManager.DownSwipe -= GestureManager_DownSwipe;
        GestureManager.LeftSwipe -= GestureManager_LeftSwipe;
        GestureManager.RightSwipe -= GestureManager_RightSwipe;
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
        FiguresParent.DestroyAllChilds();
        FigureGenerator.Setup(AvailableFiguresTypes, GridData.Width, GridData.Height, scale);
        var figures = FigureGenerator.AlternativeAutomergelessInitalFigures(FiguresParent);
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
            CheckTurnAvailable();
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
        var scores = 0;
        for (int i = GridData.Width - 1; i >= 0; i--)
        {
            for (int j = GridData.Height - 1; j >= 0; j--)
            {
                if (figures[i][j].TileData.IsForDelete)
                {
                   
                    Destroy(figures[i][j].gameObject);
                     figures[i].Remove(figures[i][j]);
                    scores++;
                }
                    
            }
        }
        var levelHandler = GameObject.FindGameObjectWithTag("LevelHandler").GetComponent<LevelHandler>();
        levelHandler.AddScores(scores);

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

    private void CheckTurnAvailable()
    {
        var available = AvailableTurnExist();
        Debug.Log($"available: {available}");
        if (!available)
        {
            Debug.Log("No more turn");
            var levelHandler = GameObject.FindGameObjectWithTag("LevelHandler").GetComponent<LevelHandler>();
            levelHandler.ResetScores();
            InitFigures();
            Turn();

        }
    }

    private bool AvailableTurnExist()
    {
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                if (AvailableTurnForTile(i, j))
                {
                    Debug.Log($"{i} {j}");
                    return true;
                }
            }
        }

        return false;
    }

    public void Select(TileObject tile)
    {
        Debug.Log("Select: " + tile.TileData.ToString());
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

    public void SwipeHandle(DraggedDirection draggedDirection)
    {
        if(SelectedObject != null) {
            switch (draggedDirection)
            {
                case DraggedDirection.Up:
                    if (SelectedObject.TileData.y > 0)
                    {
                        Swap(SelectedObject, figures[SelectedObject.TileData.x][SelectedObject.TileData.y - 1]);
                    }
                    break;
                case DraggedDirection.Down:
                    if (SelectedObject.TileData.y < GridData.Height - 1)
                    {
                        Swap(SelectedObject, figures[SelectedObject.TileData.x][SelectedObject.TileData.y + 1]);
                    }
                    break;
                case DraggedDirection.Right:
                    if (SelectedObject.TileData.x < GridData.Width - 1)
                    {
                        Swap(SelectedObject, figures[SelectedObject.TileData.x + 1][SelectedObject.TileData.y]);
                    }
                    break;
                case DraggedDirection.Left:
                    if (SelectedObject.TileData.x > 0)
                    {
                        Swap(SelectedObject, figures[SelectedObject.TileData.x - 1][SelectedObject.TileData.y]);
                    }
                    break;
            }

        }

    }
    
    string FiguresType(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight) return "";
        return figures[x][y].Type;
    }

    bool HaveSameTypes(int x, int y, string type)
    {
        var figureType = FiguresType(x, y);
        if (figureType == "") return false;
        return figureType == type;
    }

    bool AvailableTurnForTile(int x, int y)
    {
        var figureType = FiguresType(x, y);
        if (figureType == "") return false;
        return (HaveSameTypes(x - 3, y, figureType) && HaveSameTypes(x - 2, y, figureType)) ||
                (HaveSameTypes(x - 1, y - 1, figureType) && HaveSameTypes(x - 2, y - 1, figureType)) ||
                (HaveSameTypes(x + 3, y - 0, figureType) && HaveSameTypes(x + 2, y - 0, figureType)) ||
                (HaveSameTypes(x - 2, y + 1, figureType) && HaveSameTypes(x - 1, y + 1, figureType)) ||
                (HaveSameTypes(x + 2, y + 1, figureType) && HaveSameTypes(x + 1, y + 1, figureType)) ||
                (HaveSameTypes(x + 2, y - 1, figureType) && HaveSameTypes(x + 1, y - 1, figureType)) ||
                (HaveSameTypes(x, y - 3, figureType) && HaveSameTypes(x, y - 2, figureType)) ||
                (HaveSameTypes(x, y + 3, figureType) && HaveSameTypes(x, y + 2, figureType)) ||
                (HaveSameTypes(x + 1, y + 1, figureType) && HaveSameTypes(x + 1, y + 2, figureType)) ||
                (HaveSameTypes(x - 1, y + 1, figureType) && HaveSameTypes(x - 1, y + 2, figureType)) ||
                (HaveSameTypes(x + 1, y - 1, figureType) && HaveSameTypes(x + 1, y - 2, figureType)) ||
                (HaveSameTypes(x - 1, y - 1, figureType) && HaveSameTypes(x - 1, y - 2, figureType)) ||
                (HaveSameTypes(x + 1, y - 1, figureType) && HaveSameTypes(x - 1, y - 1, figureType)) ||
               (HaveSameTypes(x + 1, y + 1, figureType) && HaveSameTypes(x - 1, y + 1, figureType)) ||
                (HaveSameTypes(x - 1, y - 1, figureType) && HaveSameTypes(x - 1, y + 1, figureType)) ||
                (HaveSameTypes(x + 1, y - 1, figureType) && HaveSameTypes(x + 1, y + 1, figureType));
    }


}