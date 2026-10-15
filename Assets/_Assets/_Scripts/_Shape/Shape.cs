using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject squareShapeImage;
    [HideInInspector] public ShapeData currentShapeData;
    public Vector3 shapeSelectedScale;
    [Tooltip("Gap above the pointer/finger, measured in board cells.")]
    [Range(0f, 3f)] public float dragFingerGap = 1.1f;
    private readonly List<GameObject> squares = new List<GameObject>();
    private readonly List<Vector2Int> coordinates = new List<Vector2Int>();
    public IReadOnlyList<Vector2Int> Coordinates => coordinates;
    public bool HasShape => coordinates.Count > 0;
    public Vector3 FirstSquarePosition => squares[0].transform.position;
    public Sprite BlockSprite => BlockVisual.Sprite;
    public Color BlockColor { get; private set; } = Color.white;
    public bool IsDragging => dragging;
    public float TrayCellSize { get; set; } = 38;
    private Grid grid;
    private ShapeStorage storage;
    private Vector3 homePosition, homeScale;
    private int siblingIndex;
    private bool dragging;
    private Vector2 pointerOffset;
    private Transform homeParent;
    private void Awake()
    {
        grid = FindFirstObjectByType<Grid>();
        storage = FindFirstObjectByType<ShapeStorage>();
    }
    public float GetXPositionForShapeSquare(ShapeData data, int col, Vector2 step) => (col - (data.columns - 1) / 2f) * step.x;
    private float GetYPositionForShapeSquare(ShapeData data, int row, Vector2 step) => ((data.rows - 1) / 2f - row) * step.y;
    public void CreateShape(ShapeData data)
    {
        currentShapeData = data;
        Color[] colors = { new Color(0.22f,0.62f,1), new Color(1,0.58f,0.22f), new Color(0.64f,0.4f,1), new Color(0.2f,0.8f,0.65f), new Color(1,0.38f,0.5f) };
        BlockColor = colors[Random.Range(0, colors.Length)];
        coordinates.Clear();
        if (data && data.board != null)
            for (int r = 0; r < data.rows; r++)
            for (int c = 0; c < data.columns; c++)
                if (data.board[r].column[c]) coordinates.Add(new Vector2Int(c, r));
        while (squares.Count < coordinates.Count) squares.Add(Instantiate(squareShapeImage, transform, false));
        for (int i = 0; i < squares.Count; i++) squares[i].SetActive(i < coordinates.Count);
        LayoutSquares(false);
    }
    public void RequestNewShape(ShapeData data) => CreateShape(data);
    private void LayoutSquares(bool onBoard)
    {
        if (!HasShape) return;
        var rect = squareShapeImage.GetComponent<RectTransform>();
        Vector2 step = Vector2.one * TrayCellSize;
        Vector3 scale = new Vector3(TrayCellSize * 0.9f / rect.rect.width, TrayCellSize * 0.9f / rect.rect.height, 1);
        if (onBoard && grid)
        {
            Vector3 localStep = transform.InverseTransformVector(grid.transform.TransformVector(new Vector3(grid.CellStep, grid.CellStep, 0)));
            step = new Vector2(localStep.x, localStep.y);
            Vector2 boardSize = grid.gridSquarePrefab.GetComponent<GridSquare>().normalImage.rectTransform.rect.size * grid.squaresScale;
            Vector3 localSize = transform.InverseTransformVector(grid.transform.TransformVector(boardSize));
            scale = new Vector3(localSize.x / rect.rect.width, localSize.y / rect.rect.height, 1);
        }
        for (int i = 0; i < coordinates.Count; i++)
        {
            var p = coordinates[i];
            squares[i].transform.localScale = scale;
            var displayColor = BlockColor;
            if (onBoard) displayColor.a = 0.72f;
            squares[i].GetComponent<UnityEngine.UI.Image>().color = displayColor;
            squares[i].GetComponent<UnityEngine.UI.Image>().sprite = BlockSprite;
            squares[i].transform.localPosition = new Vector3(GetXPositionForShapeSquare(currentShapeData, p.x, step), GetYPositionForShapeSquare(currentShapeData, p.y, step), 0);
        }
    }
    public void OnBeginDrag(PointerEventData data)
    {
        if (!HasShape || !grid || grid.IsGameOver) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, data.position, data.pressEventCamera, out var pointer)) return;
        homePosition = transform.localPosition;
        homeScale = transform.localScale;
        siblingIndex = transform.GetSiblingIndex();
        homeParent = transform.parent;
        dragging = true;
        if (shapeSelectedScale.x > 0 && shapeSelectedScale.y > 0) transform.localScale = shapeSelectedScale;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas) transform.SetParent(canvas.rootCanvas.transform, true);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, data.position, data.pressEventCamera, out pointer);
        pointerOffset = (Vector2)transform.localPosition - pointer;
        // Put the entire shape above the finger, including tall blocks.
        Vector3 boardStep = transform.parent.InverseTransformVector(
            grid.transform.TransformVector(Vector3.up * grid.CellStep));
        pointerOffset.y = Mathf.Abs(boardStep.y) * (currentShapeData.rows * 0.5f + dragFingerGap);
        transform.SetAsLastSibling();
        LayoutSquares(true);
        OnDrag(data);
    }
    public void OnDrag(PointerEventData data)
    {
        if (!dragging) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform.parent, data.position, data.pressEventCamera, out var p))
            transform.localPosition = new Vector3(p.x + pointerOffset.x, p.y + pointerOffset.y, homePosition.z);
        grid.ShowPreview(this);
    }
    public void OnEndDrag(PointerEventData data)
    {
        if (!dragging) return;
        OnDrag(data);
        bool placed = grid.TryPlace(this);
        ReturnHome();
        if (!placed) return;
        foreach (var square in squares) square.SetActive(false);
        coordinates.Clear();
        currentShapeData = null;
        if (storage) storage.OnShapePlaced();
    }
    private void ReturnHome()
    {
        dragging = false;
        if (grid) grid.ClearPreview();
        transform.SetParent(homeParent, false);
        transform.localPosition = homePosition;
        transform.localScale = homeScale;
        transform.SetSiblingIndex(siblingIndex);
        LayoutSquares(false);
    }
    private void OnDisable() { if (dragging) ReturnHome(); }
    public void RefreshTrayLayout() { if (!dragging) LayoutSquares(false); }
}
