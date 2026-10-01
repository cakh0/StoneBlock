using UnityEngine;


 [CreateAssetMenu]
 [System.Serializable]
public class ShapeData : ScriptableObject
{
   [System.Serializable]
    public class Row
    {
        //1. Row properties
        public bool[] column;
        private int size = 0;
        public Row() { }

        //2. Row constructor
        public Row(int size)
        {
            CreateRow(size);
        }

        public void CreateRow(int size)
        {
            this.size = size;
            column = new bool[size];
            ClearRow();
        }
        //3. Clear the row by setting all columns to false
        public void ClearRow()
        {
            for (int i = 0; i < size; i++)
            {
                column[i] = false;
            }
        }
    }
    
    //4. ShapeData properties
    public int columns = 0;
    public int rows = 0;
    public Row[] board;
    public void Clear()
    {
        for (int i = 0; i < rows; i++)
        {
            board[i].ClearRow();
        }
    }
    public void CreateNewBoard()
    {
        board = new Row[rows];
        for (int i = 0; i < rows; i++)
        {
            board[i] = new Row(columns);
        }
    }
}

