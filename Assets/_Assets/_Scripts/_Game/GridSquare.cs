using UnityEngine;
public class GridSquare : MonoBehaviour
{
    public UnityEngine.UI.Image normalImage;
    public int squareIndex { get; set; }
    public bool IsOccupied { get; private set; }
    private UnityEngine.UI.Image blockImage;
    private UnityEngine.UI.Image previewImage;
    private Color normalColor;
    private void Awake()
    {
        normalColor = normalImage.color;
        normalImage.raycastTarget = false;
        var go = new GameObject("PlacedBlock", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(normalImage.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        blockImage = go.GetComponent<UnityEngine.UI.Image>();
        blockImage.raycastTarget = false;
        go.SetActive(false);
        var ghost = new GameObject("PlacementPreview", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Outline));
        ghost.transform.SetParent(normalImage.transform, false);
        var ghostRect = ghost.GetComponent<RectTransform>();
        ghostRect.anchorMin = Vector2.zero;
        ghostRect.anchorMax = Vector2.one;
        ghostRect.offsetMin = ghostRect.offsetMax = Vector2.zero;
        // Slightly larger than the placed tile, so the rim remains visible while dragging.
        ghostRect.localScale = Vector3.one * 1.08f;
        previewImage = ghost.GetComponent<UnityEngine.UI.Image>();
        previewImage.raycastTarget = false;
        var outline = ghost.GetComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0.85f, 0.96f, 1f, 0.95f);
        outline.effectDistance = new Vector2(3f, -3f);
        ghost.SetActive(false);
    }
    public void SetPreview(bool show)
    {
        normalImage.color = normalColor;
        previewImage.gameObject.SetActive(show && !IsOccupied);
    }
    public void ShowPreview(Sprite sprite, Color color)
    {
        previewImage.sprite = sprite;
        color = Color.Lerp(color, Color.white, 0.3f);
        color.a = 0.65f;
        previewImage.color = color;
        SetPreview(true);
    }
    public void SetOccupied(Sprite sprite, Color color)
    {
        IsOccupied = true;
        SetPreview(false);
        blockImage.sprite = sprite;
        blockImage.color = color;
        blockImage.gameObject.SetActive(true);
    }
    public void Clear() { IsOccupied = false; blockImage.gameObject.SetActive(false); SetPreview(false); }
}
