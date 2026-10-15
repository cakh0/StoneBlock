using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Grid : MonoBehaviour
{
    public int columns = 8, rows = 8;
    public float squaresGap = 0.1f;
    public GameObject gridSquarePrefab;
    public Vector2 startPosition;
    public float squaresScale = 0.5f, everySquareOffset;
    public TMP_Text scoreText;
    public int pointsPerSquare = 1, pointsPerLine = 10;
    [Tooltip("Maximum distance, in cells, to snap to the nearest valid placement.")]
    [Range(0.75f, 2f)] public float placementSnapRadius = 1.25f;
    public int Score { get; private set; }
    public float CellStep { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsReady => cells.Count == rows * columns && cells.Count > 0;
    private readonly List<GridSquare> cells = new List<GridSquare>();
    private readonly List<int> preview = new List<int>();

    private void Start()
    {
        if (!gridSquarePrefab || rows <= 0 || columns <= 0) return;
        CellStep = gridSquarePrefab.GetComponent<RectTransform>().rect.width * squaresScale + squaresGap;
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < columns; c++)
        {
            var go = Instantiate(gridSquarePrefab, transform, false);
            go.transform.localScale = Vector3.one * squaresScale;
            go.transform.localPosition = startPosition + new Vector2(c * CellStep, -r * CellStep);
            var cell = go.GetComponent<GridSquare>();
            cell.squareIndex = cells.Count;
            cells.Add(cell);
        }
        if (!scoreText)
        {
            var go = new GameObject("Score", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(columns * CellStep, 70);
            rect.localPosition = new Vector3(startPosition.x + (columns - 1) * CellStep / 2, startPosition.y + CellStep, 0);
            scoreText = go.GetComponent<TextMeshProUGUI>();
            scoreText.fontSize = 44;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.raycastTarget = false;
        }
        UpdateScore();
        if (!GetComponent<BlockGameUI>()) gameObject.AddComponent<BlockGameUI>();
    }

    public bool GetPlacement(Shape shape, List<int> result)
    {
        result.Clear();
        if (IsGameOver || cells.Count == 0 || !shape.HasShape) return false;
        Vector3 p = transform.InverseTransformPoint(shape.FirstSquarePosition);
        float pointerColumn = (p.x - startPosition.x) / CellStep;
        float pointerRow = (startPosition.y - p.y) / CellStep;
        Vector2Int first = shape.Coordinates[0];
        float bestDistance = placementSnapRadius * placementSnapRadius;
        int bestRow = -1, bestColumn = -1;
        // Use the same nearest-valid search for the ghost and the actual drop.
        // This also catches placements just outside the board's edges.
        for (int row = 0; row < rows; row++)
        for (int column = 0; column < columns; column++)
        {
            float dx = column + first.x - pointerColumn;
            float dy = row + first.y - pointerRow;
            float distance = dx * dx + dy * dy;
            if (distance > bestDistance) continue;
            bool fits = true;
            foreach (var offset in shape.Coordinates)
            {
                int r = row + offset.y, c = column + offset.x;
                if (r < 0 || r >= rows || c < 0 || c >= columns || cells[r * columns + c].IsOccupied)
                { fits = false; break; }
            }
            if (!fits) continue;
            bestDistance = distance;
            bestRow = row;
            bestColumn = column;
        }
        if (bestRow < 0) return false;
        foreach (var offset in shape.Coordinates)
            result.Add((bestRow + offset.y) * columns + bestColumn + offset.x);
        return result.Count > 0;
    }
    public void ShowPreview(Shape shape)
    {
        ClearPreview();
        if (GetPlacement(shape, preview))
            foreach (int i in preview) cells[i].ShowPreview(shape.BlockSprite, shape.BlockColor);
    }
    public void ClearPreview()
    {
        foreach (int i in preview) cells[i].SetPreview(false);
        preview.Clear();
    }
    public bool TryPlace(Shape shape)
    {
        ClearPreview();
        var placement = new List<int>();
        if (!GetPlacement(shape, placement)) return false;
        foreach (int i in placement) cells[i].SetOccupied(shape.BlockSprite, shape.BlockColor);
        // Collect all lines before clearing: intersections must clear simultaneously.
        var clear = new HashSet<int>();
        int lines = 0;
        for (int r = 0; r < rows; r++)
        {
            bool full = true;
            for (int c = 0; c < columns; c++) full &= cells[r * columns + c].IsOccupied;
            if (!full) continue;
            lines++;
            for (int c = 0; c < columns; c++) clear.Add(r * columns + c);
        }
        for (int c = 0; c < columns; c++)
        {
            bool full = true;
            for (int r = 0; r < rows; r++) full &= cells[r * columns + c].IsOccupied;
            if (!full) continue;
            lines++;
            for (int r = 0; r < rows; r++) clear.Add(r * columns + c);
        }
        foreach (int i in clear) cells[i].Clear();
        Score += placement.Count * pointsPerSquare + lines * pointsPerLine;
        UpdateScore();
        return true;
    }
    private void UpdateScore() { if (scoreText) scoreText.text = "Score: " + Score; }

    public bool CanFit(Shape shape)
    {
        if (!IsReady || !shape.HasShape) return false;
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < columns; c++)
        {
            bool fits = true;
            foreach (var p in shape.Coordinates)
            {
                int rr = r + p.y, cc = c + p.x;
                if (rr >= rows || cc >= columns || cells[rr * columns + cc].IsOccupied) { fits = false; break; }
            }
            if (fits) return true;
        }
        return false;
    }

    public void EndGame() { IsGameOver = true; ClearPreview(); }
    public void RestartGame()
    {
        foreach (var cell in cells) cell.Clear();
        IsGameOver = false;
        Score = 0;
        UpdateScore();
        FindFirstObjectByType<ShapeStorage>().ResetTray();
    }

    public void ResizeBoard(float width)
    {
        float step = width / columns;
        CellStep = step;
        float visualWidth = gridSquarePrefab.GetComponent<GridSquare>().normalImage.rectTransform.rect.width;
        squaresScale = step * 0.9f / visualWidth;
        startPosition = new Vector2(-(columns - 1) * step / 2, (rows - 1) * step / 2);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].transform.localScale = Vector3.one * squaresScale;
            cells[i].transform.localPosition = startPosition + new Vector2(i % columns * step, -(i / columns) * step);
        }
    }
}
