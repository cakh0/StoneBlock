using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Shape : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    //1. Shape properties
    public  GameObject squareShapeImage;
    [HideInInspector] 
    public ShapeData currentShapeData;
    private List<GameObject> currentShapeSquares = new List<GameObject>();

    public Vector3 shapeSelectedScale;

    //2. Initialize the shape with the provided ShapeData
    private int GetNumberOfSquares(ShapeData shapeData)
    {
        int count = 0;
        foreach (var row in shapeData.board)
        {
            foreach (var active in row.column)
            {
                if (active)
                {
                    count++;
                }
            }
        }
        return count;
    }
    //3. Create the shape squares based on the ShapeData
     public float GetXPositionForShapeSquare(ShapeData shapeData, int column, Vector2 moveDistance)
    {
        // Tính vị trí X dựa trên column index
        // Column 0 ở bên trái, column tăng dần sang phải
        // Trung tâm shape nằm ở giữa

        int totalColumns = shapeData.columns;

        // Tính offset từ trung tâm
        // Với số lẻ: trung tâm là (totalColumns - 1) / 2
        // Với số chẵn: trung tâm nằm giữa 2 ô

        float centerColumn = (totalColumns - 1) / 2f;
        float offsetFromCenter = column - centerColumn; // Dương = sang phải, Âm = sang trái

        return offsetFromCenter * moveDistance.x;
    }

    //4. Calculate the Y position for a shape square based on its row index and the move distance
    private float GetYPositionForShapeSquare(ShapeData shapeData, int row, Vector2 moveDistance)
    {
        // Tính vị trí Y dựa trên row index
        // Row 0 ở trên cùng, row tăng dần đi xuống
        // Trung tâm shape nằm ở giữa

        int totalRows = shapeData.rows;

        // Tính offset từ trung tâm
        // Với số lẻ: trung tâm là (totalRows - 1) / 2
        // Với số chẵn: trung tâm nằm giữa 2 ô

        float centerRow = (totalRows - 1) / 2f;
        float offsetFromCenter = centerRow - row; // Dương = lên trên, Âm = xuống dưới

        return offsetFromCenter * moveDistance.y;      
    }

    //5. Create the shape based on the provided ShapeData, activating and positioning the squares accordingly
    public void CreateShape(ShapeData shapeData)
    {
        currentShapeData = shapeData;
        var totalSquares = GetNumberOfSquares(shapeData);
        while (currentShapeSquares.Count <= totalSquares)
        {
            var newSquare = Instantiate(squareShapeImage, transform);
            currentShapeSquares.Add(newSquare);
        }
        foreach (var square in currentShapeSquares)
        {
            square.gameObject.transform.position = Vector3.zero;
            square.gameObject.SetActive(false);
        }
        var squareRect = squareShapeImage.GetComponent<RectTransform>();
        var moveDistance = new Vector2(squareRect.rect.width * squareRect.localScale.x, squareRect.rect.height * squareRect.localScale.y);

        int currentSquareIndex = 0;
        for (int row = 0; row < shapeData.rows; row++)
        {
            for (int col = 0; col < shapeData.columns; col++)
            {
                if (shapeData.board[row].column[col])
                {
                    var square = currentShapeSquares[currentSquareIndex];
                    square.gameObject.SetActive(true);
                    square.gameObject.GetComponent<RectTransform>().localPosition = new Vector2(GetXPositionForShapeSquare(shapeData, col, moveDistance), GetYPositionForShapeSquare(shapeData, row, moveDistance));
                    currentSquareIndex++;
                }
            }
        }
    }

    //6. Request a new shape to be created based on the provided ShapeData
    public void RequestNewShape(ShapeData shapeData)
    {
        CreateShape(shapeData);
    }


    void Start()
    {
        // //7. If currentShapeData is not null, request a new shape to be created at the start
        if (currentShapeData != null)
        {
             RequestNewShape(currentShapeData);
        }
    }
    //8. Update is called once per frame
    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle pointer click event
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Handle pointer up event
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Handle begin drag event
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Handle drag event
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Handle end drag event
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Handle pointer down event
    }

    
   
}
