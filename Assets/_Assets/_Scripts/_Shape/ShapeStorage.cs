using UnityEngine;
using System.Collections.Generic;

public class ShapeStorage : MonoBehaviour
{   
    //1. ShapeStorage properties
    public List<ShapeData> shapeData;
    public List<Shape> shapeList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var shape in shapeList)
        {
            //2. Create a new shape for each ShapeData in the shapeData list
            var shapeIndex = UnityEngine.Random.Range(0, shapeData.Count);
            shape.CreateShape(shapeData[shapeIndex]);
        }
    }

   
}
