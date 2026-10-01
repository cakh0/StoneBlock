using UnityEngine;
using UnityEngine.UI;

public class ShapeSquares : MonoBehaviour
{
    //1. ShapeSquare properties
    [Header("Occupied State")]
    public Image occupiedImage;

    void Start()
    {
        //2. Khi Start, đảm bảo rằng các trạng thái hình ảnh được thiết lập đúng trạng thái
        occupiedImage.gameObject.SetActive(false);
    }

    public void DeactivateShape()
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        gameObject.SetActive(false);
    }

    public void ActivateShape()
    {
        gameObject.GetComponent<BoxCollider2D>().enabled = true;
        gameObject.SetActive(true);
    }

    public void SetOccupied()
    {
        //occupiedImage.gameObject.SetActive(true);
    }

    public void UnSetOccupied()
    {
        //occupiedImage.gameObject.SetActive(false);
    }

}

