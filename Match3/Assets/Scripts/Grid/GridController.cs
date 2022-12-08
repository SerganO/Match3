using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[SelectionBase]
public class GridController : MonoBehaviour
{
    public enum BoosterType
    {
        Vertical, Horizontal, Multi, Bomb
    }

    public enum LogicBooster {
        None,
        Vertical, Horizontal, Multi, Bomb,

        DoubleStraight,
        MultiVertical,
        VerticalBomb,
        MultiHorizontal,
        HorizontalBomb,
        Total,
        MultiBomb,
        HighBomb

    }


    class BoosterPoint {
        public int x;
        public int y;
        public string boosterType;

        public BoosterPoint(int x, int y, string boosterType)
        {
            this.x = x;
            this.y = y;
            this.boosterType = boosterType;
        }
    }

    public bool turnInProcess = false;
    public event VoidFunc BuildCompleted;
    public GridFigureGenerator FigureGenerator;
    public GestureManager GestureManager;
    public Canvas Canvas;

    [Header("UI")]
    public GameObject Grid;
    public GameObject GridLine;
    public GridCell GridCell;

    public RectTransform MessageContainer;

    public RectTransform UIContainer;

    [Header("Data")]
    public GridData GridData;

    List<string> allBoostersList = new List<string> { 
        "Vertical",
        "Horizontal",
        "Multi",
        "Bomb"
    
    };

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

            var offsetData = ScreenManager.Shared.ScaleOrientation(GridData, UIContainer, Canvas);

            switch (offsetData.Item1)
            {
                case ScreenManager.PositionOrientation.Horizontal:
                    yDivider /= offsetData.Item2;
                    break;
                case ScreenManager.PositionOrientation.Vertical:
                    xDivider /= offsetData.Item2;
                    break;
            }

            scale = ScreenManager.Shared.scaleForGrid(GridData, UIContainer, Canvas);
            var gridObject = BuildFuncStep();
            var center = UIContainer.gameObject.transform.position;
            gridObject.transform.position = new Vector3(center.x - ScreenManager.Shared.MapWidthScreenToWorld(UIContainer.rect.width * Canvas.scaleFactor) / xDivider + scale / 2,
                center.y - ScreenManager.Shared.MapHeightScreenToWorld(UIContainer.rect.height * Canvas.scaleFactor) / yDivider + scale / 2);
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


    public void Turn(bool forceCheck = false)
    {
        turnInProcess = true;
        if (Check() || forceCheck)
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
        var boosterPoints = new List<BoosterPoint>();
        var figuresForDeletes = new List<List<Vector2Int>>();
        var currentDeleteLine = new List<Vector2Int>();
        //VerticalCheck
        for (int i = 0; i < GridData.Width; i++)
        {
            var lastType = "";
            var count = 0;
            var preX = -1;
            var preY = -1;
            currentDeleteLine = new List<Vector2Int>();
            for (int j = 0; j < GridData.Height; j++)
            {
                var figure = figures[i][j];
                var figureType = figure.Type;
                
                if (!figure.TileData.IsForDelete || lastType != figureType)
                {
                    if (count == 4)
                    {
                        boosterPoints.Add(new BoosterPoint(preX, preY, "Horizontal"));
                    }
                    else if (count > 4)
                    {
                        boosterPoints.Add(new BoosterPoint(preX, preY, "Multi"));
                    }
                    if (count >= 3)
                    {
                        figuresForDeletes.Add(currentDeleteLine);
                    }

                    lastType = figureType;
                    count = 1;
                    preX = -1;
                    preY = -1;
                    currentDeleteLine = new List<Vector2Int>(){
                        new Vector2Int(i, j) 
                    };
    
                }
                else
                {
                    count++;
                    currentDeleteLine.Add(new Vector2Int(i, j));
                    if (preX == -1) preX = i;
                    else if (figure.TileData.IsMoved) preX = i;
                    if (preY == -1) preY = j;
                    else if (figure.TileData.IsMoved) preY = j;
                }
            }
            if (count >= 3)
            {
                figuresForDeletes.Add(currentDeleteLine);
            }

            if (count == 4)
            {
                boosterPoints.Add(new BoosterPoint(preX, preY, "Horizontal"));
            }
            else if (count > 4)
            {
                boosterPoints.Add(new BoosterPoint(preX, preY, "Multi"));
            }
        }

        currentDeleteLine = new List<Vector2Int>();
        //HorizontalCheck
        for (int j = 0; j < GridData.Height; j++)
        {
            var lastType = "";
            var count = 0;
            var preX = -1;
            var preY = -1;
            currentDeleteLine = new List<Vector2Int>();
            for (int i = 0; i < GridData.Width; i++)
            {
                var figure = figures[i][j];
                var figureType = figure.Type;
                if (!figure.TileData.IsForDelete || lastType != figureType)
                {
                    if (count == 4)
                    {
                        boosterPoints.Add(new BoosterPoint(preX, preY, "Vertical"));
                    }
                    else if (count > 4)
                    {
                        boosterPoints.Add(new BoosterPoint(preX, preY, "Multi"));
                    }
                    if(figuresForDeletes.Count > 0 && count >= 3)
                    {
                        var intersect = figuresForDeletes.Find(list => { return list.Intersect(currentDeleteLine).ToList().Count > 0; });
                        if (intersect != null)
                        {
                            if (boosterPoints.Find(point => point.x == intersect.First().x && point.y == intersect.First().y) == null)
                                boosterPoints.Add(new BoosterPoint(intersect.First().x, intersect.First().y, "Bomb"));
                        }
                    }
                    

                    lastType = figureType;
                    count = 1;
                    currentDeleteLine = new List<Vector2Int>
                    {
                        new Vector2Int(i, j)
                    };
                    preX = -1;
                    preY = -1;
                }
                else
                {
                    count++;
                    currentDeleteLine.Add(new Vector2Int(i, j));
                    if (preX == -1) preX = i;
                    else if (figure.TileData.IsMoved) preX = i;
                    if (preY == -1) preY = j;
                    else if (figure.TileData.IsMoved) preY = j;
                }
            }

            
            if (count == 4)
            {
                boosterPoints.Add(new BoosterPoint(preX, preY, "Vertical"));
            }
            else if (count > 4)
            {
                boosterPoints.Add(new BoosterPoint(preX, preY, "Multi"));
            }
            if (figuresForDeletes.Count > 0 && count >= 3)
            {
                var intersect = figuresForDeletes.Find(list => { return list.Intersect(currentDeleteLine).ToList().Count > 0; });
                if (intersect != null)
                {
                    if (boosterPoints.Find(point => point.x == intersect.First().x && point.y == intersect.First().y) == null)
                        boosterPoints.Add(new BoosterPoint(intersect.First().x, intersect.First().y, "Bomb"));
                }
            }
        }
        var scores = 0;
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
                    var boosterPoint = boosterPoints.Find(point => point.x == i && point.y == j);
                    if(boosterPoint != null)
                    {
                        var obj = BaseResourcesSuppliers.PrefabsSupplier.GetObjectForID(boosterPoint.boosterType, "Booster").GetComponent<TileObject>();
                        var booster = Instantiate(obj, FiguresParent);
                        booster.transform.localScale = new Vector3(scale, scale, 1);
                        booster.TileData.x = i;
                        booster.TileData.y = j;
                        Destroy(figures[i][j].gameObject);
                        figures[i][j] = booster;
                        scores++;

                    } else
                    {
                        for (int h = j - 1; h >= 0; h--)
                        {
                            if (!figures[i][h].TileData.IsForDelete && !figures[i][h].TileData.IsMoved)
                            {
                                figures[i][h].TileData.y = j;
                                figures[i][h].TileData.IsMoved = true;
                                break;
                            }
                        }
                        addList[i]++;
                    }
                   
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
                figures[i][j].TileData.IsMoved = false;
            }
        }


    }

    public void MoveToIndexes(int x, int y)
    {
        var startPoint = TopLeftCell.transform.position;
        figures[x][y].transform.position = new Vector3(startPoint.x + scale * x, startPoint.y - scale * y, figures[x][y].transform.position.z);
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
            tile1.TileData.IsMoved = true;
            tile2.TileData.IsMoved = true;
            var firstIsBoost = allBoostersList.Contains(tile1.TileData.Type);
            var secondIsBoost = allBoostersList.Contains(tile2.TileData.Type);

            if (firstIsBoost || secondIsBoost)
            {
                if(firstIsBoost && secondIsBoost)
                {
                    HandleBoostSwap(tile1, tile2, true);
                } else if (firstIsBoost)
                {
                    HandleBoostSwap(tile1, tile2, false);
                } else
                {
                    HandleBoostSwap(tile2, tile1, false);
                }

                Turn(true);
            }
            else
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
            }


           
        } else {
            tile1.TileData.IsMoved = false;
            tile2.TileData.IsMoved = false;
            turnInProcess = false;
        }


    }

    public void HandleBoostAuto(TileObject boostTile)
    {
        var boosterType = BoosterType.Parse<BoosterType>(boostTile.Type);
        boostTile.TileData.IsForDelete = true;
        switch (boosterType)
        {
            case BoosterType.Vertical:
            case BoosterType.Horizontal:
                HandleHorizontal(boostTile.Y);
                HandleVertical(boostTile.X);
                break;
            case BoosterType.Multi:
                HandleBomb(boostTile, 4);
                break;
            case BoosterType.Bomb:
                HandleBomb(boostTile, 2);
                break;

        }
    }

    public LogicBooster GetBoosterResult(TileObject tile1, TileObject tile2)
    {
        var compositeType = tile1.Type + tile2.Type;
        switch(compositeType)
        {
            case "VerticalVertical":
            case "VerticalHorizontal":
            case "HorizontalVertical":
            case "HorizontalHorizontal":
                return LogicBooster.DoubleStraight;
            case "MultiMulti":
                return LogicBooster.Total;
            case "BombBomb":
                return LogicBooster.HighBomb;
            case "MultiBomb":
            case "BombMulti":
                return LogicBooster.MultiBomb;
            case "VerticalBomb":
            case "BombVertical":
                return LogicBooster.VerticalBomb;
            case "HorizontalBomb":
            case "BombHorizontal":
                return LogicBooster.VerticalBomb;
            case "MultiVertical":
            case "VerticalMulti":
                return LogicBooster.MultiVertical;
            case "MultiHorizontal":
            case "HorizontalMulti":
                return LogicBooster.MultiHorizontal;

        }


        return LogicBooster.None;
    }

    public void HandleBoostSwap(TileObject boostTile, TileObject swapPair, bool isDoubleBoost)
    {

        if (isDoubleBoost)
        {
            boostTile.TileData.IsForDelete = true;
            swapPair.TileData.IsForDelete = true;
            var boosterType = GetBoosterResult(boostTile, swapPair);
            switch (boosterType)
            {
                case LogicBooster.None:
                    break;
                case LogicBooster.Vertical:
                    HandleVertical(swapPair.X);
                    break;
                case LogicBooster.Horizontal:
                    HandleHorizontal(swapPair.Y);
                    break;
                case LogicBooster.Multi:
                    HandleMulti(swapPair.Type);
                    break;
                case LogicBooster.Bomb:
                    var radius = 2;
                    HandleBomb(swapPair, radius);
                    break;
                case LogicBooster.DoubleStraight:
                    HandleVertical(swapPair.X);
                    HandleHorizontal(swapPair.Y);
                    break;
                case LogicBooster.MultiVertical:
                    var isXOdd = swapPair.X % 2 == 0;
                    for (int i = 0; i < gridWidth; i++)
                    {
                        if (isXOdd)
                        {
                            if (i % 2 == 0) HandleVertical(i);
                        }
                        else
                        {
                            if (i % 2 != 0) HandleVertical(i);
                        }
                    }
                    break;
                case LogicBooster.VerticalBomb:
                    var x = swapPair.X;
                    if (x > 0)
                    {
                        HandleVertical(swapPair.X - 1);
                    }
                    HandleVertical(swapPair.X);
                    if (x < gridWidth - 1)
                    {
                        HandleVertical(swapPair.X + 1);
                    }
                    break;
                case LogicBooster.MultiHorizontal:
                    var isYOdd = swapPair.Y % 2 == 0;
                    for(int j =0;j<gridHeight;j++)
                    {
                        if(isYOdd)
                        {
                            if (j % 2 == 0) HandleHorizontal(j);
                        } else
                        {
                            if (j % 2 != 0) HandleHorizontal(j);
                        }
                    }
                    break;
                case LogicBooster.HorizontalBomb:
                    var y = swapPair.Y;
                    if (y > 0)
                    {
                        HandleHorizontal(swapPair.Y - 1);
                    }
                    HandleHorizontal(swapPair.Y);
                    if (y < gridHeight - 1)
                    {
                        HandleHorizontal(swapPair.Y + 1);
                    }
                    break;
                case LogicBooster.Total:
                    HandleTotal();
                    break;
                case LogicBooster.MultiBomb:
                    var mRadiuis = (gridWidth + gridHeight) * 3 / 8;
                    HandleBomb(swapPair, mRadiuis);
                    break;
                case LogicBooster.HighBomb:
                    var hRadius = 4;
                    HandleBomb(swapPair, hRadius);
                    break;
            }

        }
        else
        {
            boostTile.TileData.IsForDelete = true;
            var boosterType = BoosterType.Parse<BoosterType>(boostTile.Type);
            switch (boosterType)
            {
                case BoosterType.Vertical:
                    HandleVertical(swapPair.X);
                    break;
                case BoosterType.Horizontal:
                    HandleHorizontal(swapPair.Y);
                    break;
                case BoosterType.Multi:
                    HandleMulti(swapPair.Type);
                    break;
                case BoosterType.Bomb:
                    var radius = 2;
                    HandleBomb(swapPair, radius);
                    break;
            }
        }
    }

    private void HandleVertical(int line)
    {
        for (int j = 0; j < gridHeight; j++)
        {
            if (IsBoostAutoTurned(line, j))
            {
                HandleBoostAuto(figures[line][j]);
            }
            else
            {
                figures[line][j].TileData.IsForDelete = true;
            }
        }
    }

    private void HandleHorizontal(int line)
    {
        for (int i = 0; i < gridWidth; i++)
        {
            if (IsBoostAutoTurned(i, line))
            {
                HandleBoostAuto(figures[i][line]);
            }
            else
            {
                figures[i][line].TileData.IsForDelete = true;
            }
        }
    }

    private void HandleMulti(string type)
    {
        figures.ForEach(figList =>
        {
            figList.ForEach(fig =>
            {
                if (fig.Type == type)
                {
                    fig.TileData.IsForDelete = true;
                }

            });
        });
    }

    private void HandleTotal()
    {
        figures.ForEach(figList =>
        {
            figList.ForEach(fig =>
            {
                fig.TileData.IsForDelete = true;
            });
        });
    }

    private void HandleBomb(TileObject swapPair, int radius)
    {
        int x = swapPair.X;
        int y = swapPair.Y;
        int startX = Mathf.Max(0, x - radius);
        int startY = Mathf.Max(0, y - radius);
        int finalX = Mathf.Min(gridWidth - 1, x + radius);
        int finalY = Mathf.Min(gridHeight - 1, y + radius);
        for (int i = startX; i <= finalX; i++)
        {
            for (int j = startY; j <= finalY; j++)
            {
                if (IsBoostAutoTurned(i, j))
                {
                    HandleBoostAuto(figures[i][j]);
                }
                else
                {
                    figures[i][j].TileData.IsForDelete = true;
                }
            }
        }
    }

    private bool IsBoostAutoTurned(int x, int y)
    {
        return allBoostersList.Contains(figures[x][y].Type) && !figures[x][y].TileData.IsForDelete;
    }

   /* public void HandleBoost(TileObject boostTile, TileObject swapPair)
    {
        boostTile.TileData.IsForDelete = true;
        switch(boostTile.Type)
        {
            case "Multi":
                figures.ForEach(figList =>
                {
                    figList.ForEach(fig =>
                    {
                        if(fig.Type == swapPair.Type)
                        {
                            fig.TileData.IsForDelete = true;
                        }

                    });


                });
                return;
            case "Vertical":
                for(int j = 0; j < gridHeight;j++)
                {
                    if (allBoostersList.Contains(figures[swapPair.X][j].Type) && !figures[swapPair.X][j].TileData.IsForDelete)
                    {
                        switch (figures[swapPair.X][j].Type)
                        {

                            case "Multi":
                                HandleMultiStraight(figures[swapPair.X][j], figures[swapPair.X][j]);
                                return;
                            case "Vertical":
                                HandleDoubleStraight(figures[swapPair.X][j], figures[swapPair.X][j]);
                                return;
                            case "Horizontal":
                                HandleDoubleStraight(figures[swapPair.X][j], figures[swapPair.X][j]);
                                return;
                        }
                    }
                    else
                    {
                        figures[swapPair.X][j].TileData.IsForDelete = true;
                    }
                   
                }
                return;
            case "Horizontal":
                for (int i = 0; i < gridWidth; i++)
                {
                    if (allBoostersList.Contains(figures[i][swapPair.Y].Type) && !figures[i][swapPair.Y].TileData.IsForDelete)
                    {
                        switch (figures[i][swapPair.Y].Type)
                        {

                            case "Multi":
                                HandleMultiStraight(figures[i][swapPair.Y], figures[i][swapPair.Y]);
                                return;
                            case "Vertical":
                                HandleDoubleStraight(figures[i][swapPair.Y], figures[i][swapPair.Y]);
                                return;
                            case "Horizontal":
                                HandleDoubleStraight(figures[i][swapPair.Y], figures[i][swapPair.Y]);
                                return;
                        }
                    }
                    else
                    {
                        figures[i][swapPair.Y].TileData.IsForDelete = true;
                    }
                   
                }
                break;

        }
    }

    public void HandleDoubleBoost(TileObject boostTile, TileObject swapPair)
    {
        boostTile.TileData.IsForDelete = true;
        swapPair.TileData.IsForDelete = true;
        switch (boostTile.Type)
        {
            case "Multi":
                switch (swapPair.Type)
                {
                    case "Multi":
                        figures.ForEach(figList =>
                        {
                            figList.ForEach(fig =>
                            {

                                fig.TileData.IsForDelete = true;


                            });
                        });
                        return;
                    case "Vertical":
                        HandleMultiStraight(boostTile, swapPair);
                        return;
                    case "Horizontal":
                        HandleMultiStraight(boostTile, swapPair);
                        return;

                }
                return;
            case "Vertical":
                switch (swapPair.Type)
                {
                    case "Multi":
                        HandleMultiStraight(boostTile, swapPair);
                        return;
                    case "Vertical":
                        HandleDoubleStraight(boostTile, swapPair);
                        return;
                    case "Horizontal":
                        HandleDoubleStraight(boostTile, swapPair);
                        return;

                }
                return;
            case "Horizontal":
                switch (swapPair.Type)
                {
                    case "Multi":
                        HandleMultiStraight(boostTile, swapPair);
                        return;
                    case "Vertical":
                        HandleDoubleStraight(boostTile, swapPair);
                        return;
                    case "Horizontal":
                        HandleDoubleStraight(boostTile, swapPair);
                        return;

                }
                return;

        }
    }

    void HandleMultiStraight(TileObject boostTile, TileObject swapPair)
    {
        boostTile.TileData.IsForDelete = true;
        swapPair.TileData.IsForDelete = true;
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                if(i == boostTile.X || i == boostTile.X + 1 || i == boostTile.X - 1 ||
                    i == swapPair.X || i == swapPair.X + 1 || i == swapPair.X - 1 ||
                    j == boostTile.Y || j == boostTile.Y + 1 || j == boostTile.Y - 1 ||
                    j == swapPair.Y || j == swapPair.Y + 1 || j == swapPair.Y - 1)
                {
                    if (allBoostersList.Contains(figures[i][j].Type) && !figures[i][j].TileData.IsForDelete)
                    {
                        switch(figures[i][j].Type)
                        {

                            case "Multi":
                                HandleMultiStraight(figures[i][j], figures[i][j]);
                                return;
                            case "Vertical":
                                HandleDoubleStraight(figures[i][j], figures[i][j]);
                                return;
                            case "Horizontal":
                                HandleDoubleStraight(figures[i][j], figures[i][j]);
                                return;
                        }
                    } else
                    {
                        figures[i][j].TileData.IsForDelete = true;
                    }
                    
                }
            }
        }
    }

    void HandleDoubleStraight(TileObject boostTile, TileObject swapPair)
    {
        boostTile.TileData.IsForDelete = true;
        swapPair.TileData.IsForDelete = true;
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                if (i == boostTile.X || j == boostTile.Y ||  i == swapPair.X || j == swapPair.Y)
                {
                    if (allBoostersList.Contains(figures[i][j].Type) && !figures[i][j].TileData.IsForDelete)
                    {
                        switch (figures[i][j].Type)
                        {

                            case "Multi":
                                HandleMultiStraight(figures[i][j], figures[i][j]);
                                return;
                            case "Vertical":
                                HandleDoubleStraight(figures[i][j], figures[i][j]);
                                return;
                            case "Horizontal":
                                HandleDoubleStraight(figures[i][j], figures[i][j]);
                                return;
                        }
                    }
                    else
                    {
                        figures[i][j].TileData.IsForDelete = true;
                    }
                }
            }
        }
    }*/

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
                if (allBoostersList.Contains(figures[i][j].Type) || AvailableTurnForTile(i, j))
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