using UnityEngine;
using TMPro;

// Keep the board and all three tray slots inside the safe area at any aspect ratio.
public class BlockGameUI : MonoBehaviour
{
    private Grid grid;
    private ShapeStorage storage;
    private Canvas canvas;
    private RectTransform canvasRect;
    private GameObject gameOverPanel;
    private TMP_Text finalScore;
    private Vector2 lastSize;
    private Rect lastSafeArea;
    private void Awake()
    {
        grid = GetComponent<Grid>();
        storage = FindFirstObjectByType<ShapeStorage>();
        canvas = GetComponentInParent<Canvas>().rootCanvas;
        canvasRect = (RectTransform)canvas.transform;
        BuildGameOver();
    }
    private void LateUpdate()
    {
        if (!storage || !grid.IsReady) return;
        Vector2 size = canvasRect.rect.size;
        Rect safe = Screen.safeArea;
        bool dragging = false;
        foreach (var shape in storage.shapeList) if (shape && shape.IsDragging) dragging = true;
        if (!dragging && (size != lastSize || safe != lastSafeArea))
        {
            lastSize = size; lastSafeArea = safe;
            Vector2 safeSize = new Vector2(size.x * safe.width / Screen.width, size.y * safe.height / Screen.height);
            Vector2 center = new Vector2((safe.center.x / Screen.width - 0.5f) * size.x, (safe.center.y / Screen.height - 0.5f) * size.y);
            bool landscape = safeSize.x > safeSize.y * 1.2f;
            float width = landscape ? Mathf.Min(safeSize.x * 0.55f, safeSize.y * 0.78f, 1000)
                : Mathf.Min(safeSize.x * 0.9f, safeSize.y * 0.55f, 1000);
            Vector2 boardCenter = center + (landscape ? new Vector2(-safeSize.x * 0.18f, -safeSize.y * 0.025f) : Vector2.up * safeSize.y * 0.06f);
            var rect = (RectTransform)grid.transform;
            rect.SetParent(canvas.transform, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.anchoredPosition = boardCenter;
            rect.sizeDelta = new Vector2(width, width);
            grid.ResizeBoard(width);
            var score = grid.scoreText.rectTransform;
            score.sizeDelta = new Vector2(width, 64);
            score.localPosition = new Vector3(0, width / 2 + safeSize.y * (landscape ? 0.045f : 0.065f), 0);
            grid.scoreText.fontSize = Mathf.Clamp(width * 0.075f, 28, 70);
            grid.scoreText.color = Color.white;
            float slotWidth = width / 3;
            for (int i = 0; i < storage.shapeList.Count; i++)
            {
                var shape = storage.shapeList[i];
                if (!shape) continue;
                var shapeRect = (RectTransform)shape.transform;
                // Reparent only the playable slots; preserve the surrounding scene UI.
                shapeRect.SetParent(canvas.transform, false);
                shapeRect.anchorMin = shapeRect.anchorMax = shapeRect.pivot = new Vector2(0.5f, 0.5f);
                shapeRect.localScale = Vector3.one;
                shapeRect.anchoredPosition = landscape ? center + new Vector2(safeSize.x * 0.27f, (1-i) * safeSize.y * 0.26f)
                    : center + new Vector2((i - 1) * slotWidth, safeSize.y * 0.06f - width / 2 - safeSize.y * 0.12f);
                shape.TrayCellSize = landscape ? Mathf.Min(grid.CellStep * 0.65f, safeSize.y * 0.22f / 5)
                    : Mathf.Min(grid.CellStep * 0.6f, slotWidth / 5.5f);
                shape.RefreshTrayLayout();
            }
        }
        gameOverPanel.SetActive(grid.IsGameOver);
        if (grid.IsGameOver)
        {
            gameOverPanel.transform.SetAsLastSibling();
            finalScore.text = "GAME OVER\n\nScore: " + grid.Score;
        }
    }
    private void BuildGameOver()
    {
        gameOverPanel = new GameObject("GameOver", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        gameOverPanel.transform.SetParent(canvas.transform, false);
        var panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero; panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
        gameOverPanel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.02f,0.04f,0.1f,0.94f);
        var label = new GameObject("Result", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(gameOverPanel.transform, false);
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(600,250); labelRect.anchoredPosition = new Vector2(0,100);
        finalScore = label.GetComponent<TextMeshProUGUI>();
        finalScore.fontSize = 64; finalScore.alignment = TextAlignmentOptions.Center; finalScore.raycastTarget = false;
        var button = new GameObject("PlayAgain", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        button.transform.SetParent(gameOverPanel.transform, false);
        var buttonRect = button.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(360,110); buttonRect.anchoredPosition = new Vector2(0,-140);
        button.GetComponent<UnityEngine.UI.Image>().color = new Color(0.18f,0.56f,0.95f);
        button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(grid.RestartGame);
        var text = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        text.transform.SetParent(button.transform, false);
        var textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        var tmp = text.GetComponent<TextMeshProUGUI>();
        tmp.text = "PLAY AGAIN"; tmp.fontSize = 46; tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
        gameOverPanel.SetActive(false);
    }
}
