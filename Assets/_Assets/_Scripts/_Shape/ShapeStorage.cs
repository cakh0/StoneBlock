using UnityEngine;
using System.Collections.Generic;
public class ShapeStorage : MonoBehaviour
{
    public List<ShapeData> shapeData;
    public List<Shape> shapeList;
    private Grid grid;
    private readonly List<ShapeData> generated = new List<ShapeData>();
    private void Start()
    {
        grid = FindFirstObjectByType<Grid>();
        BuildCatalog();
        Refill();
        StartCoroutine(CheckAfterStart());
    }
    private System.Collections.IEnumerator CheckAfterStart()
    {
        yield return null;
        CheckGameOver();
    }
    private void Refill()
    {
        if (shapeData == null || shapeData.Count == 0) return;
        foreach (var shape in shapeList)
            if (shape) shape.CreateShape(shapeData[Random.Range(0, shapeData.Count)]);
    }
    public void OnShapePlaced()
    {
        bool any = false;
        foreach (var shape in shapeList) if (shape && shape.HasShape) any = true;
        if (!any) Refill();
        CheckGameOver();
    }
    public void ResetTray() { Refill(); CheckGameOver(); }
    public void CheckGameOver()
    {
        if (!grid || !grid.IsReady) return;
        foreach (var shape in shapeList) if (shape && shape.HasShape && grid.CanFit(shape)) return;
        grid.EndGame();
    }
    private void BuildCatalog()
    {
        // Trim and deduplicate rotations, retaining the project's configured shapes.
        var keys = new HashSet<string>();
        var catalog = new List<ShapeData>();
        System.Action<List<Vector2Int>> add = points =>
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;
            foreach (var p in points) { minX = Mathf.Min(minX,p.x); minY = Mathf.Min(minY,p.y); }
            for (int i=0;i<points.Count;i++) { points[i] -= new Vector2Int(minX,minY); maxX=Mathf.Max(maxX,points[i].x); maxY=Mathf.Max(maxY,points[i].y); }
            points.Sort((a,b) => a.y == b.y ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            string key = string.Join(";", points);
            if (!keys.Add(key)) return;
            var data = ScriptableObject.CreateInstance<ShapeData>();
            data.name = "Block_" + catalog.Count;
            data.columns = maxX+1; data.rows=maxY+1; data.CreateNewBoard();
            foreach (var p in points) data.board[p.y].column[p.x]=true;
            generated.Add(data); catalog.Add(data);
        };
        if (shapeData != null) foreach (var data in shapeData)
        {
            if (!data || data.board == null) continue;
            var points=new List<Vector2Int>();
            for(int r=0;r<data.rows;r++) for(int c=0;c<data.columns;c++) if(data.board[r].column[c]) points.Add(new Vector2Int(c,r));
            if(points.Count>0) add(points);
        }
        string[] templates = { "1", "11", "111", "1111", "11111", "11/11", "111/111", "111/111/111", "10/11", "100/111", "100/100/111", "010/111", "110/011" };
        foreach (var template in templates)
        {
            var points = new List<Vector2Int>(); var lines=template.Split('/');
            for(int r=0;r<lines.Length;r++) for(int c=0;c<lines[r].Length;c++) if(lines[r][c]=='1') points.Add(new Vector2Int(c,r));
            for(int mirror=0;mirror<2;mirror++) for(int rotation=0;rotation<4;rotation++)
            {
                var variant=new List<Vector2Int>();
                foreach(var p in points)
                {
                    var q=new Vector2Int(mirror==0 ? p.x : -p.x,p.y);
                    for(int turn=0;turn<rotation;turn++) q=new Vector2Int(-q.y,q.x);
                    variant.Add(q);
                }
                add(variant);
            }
        }
        shapeData=catalog;
    }
    private void OnDestroy() { foreach(var data in generated) Destroy(data); }
}
