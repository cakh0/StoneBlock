using UnityEngine;

//1. import the UnityEditor namespace to access editor-specific classes and functions
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ShapeData), false)]
[CanEditMultipleObjects]
[System.Serializable]
public class ShapeDataEditor : Editor
{
    //2. Create a property to access the ShapeData instance being edited
    private ShapeData shapeDataInstance => target as ShapeData;

    //6. Override the OnInspectorGUI method to customize the inspector interface for ShapeData
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ClearBoardButton();
        EditorGUILayout.Space();
        DrawColumnsInputFields();
        EditorGUILayout.Space();
        if (shapeDataInstance.board != null && shapeDataInstance.columns > 0 && shapeDataInstance.rows > 0)
        {
            DrawBoardTable();
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(shapeDataInstance);
        }
    }

    //5. Draw a button to clear the board
    private void ClearBoardButton()
    {
        if (GUILayout.Button("Clear Board"))
        {
            shapeDataInstance.Clear();
        }
    }

    //4. Draw input fields for columns and rows, and create a new board if the values change
    private void DrawColumnsInputFields()
    {
        var columnTemp = shapeDataInstance.columns;
        var rowsTemp = shapeDataInstance.rows;

        shapeDataInstance.columns = EditorGUILayout.IntField("Columns", shapeDataInstance.columns);
        shapeDataInstance.rows = EditorGUILayout.IntField("Rows", shapeDataInstance.rows);

        if ((shapeDataInstance.columns != columnTemp || shapeDataInstance.rows != rowsTemp) &&
        shapeDataInstance.columns > 0 && shapeDataInstance.rows > 0)
        {
            shapeDataInstance.CreateNewBoard();
        }
    }
    //3. Draw the board table with toggle buttons for each cell
    private void DrawBoardTable()
    {
        var tableStyle = new GUIStyle("box");
        tableStyle.padding = new RectOffset(10, 10, 10, 10);
        tableStyle.margin.left = 32;

        var headerColumnStyle = new GUIStyle();
        headerColumnStyle.fixedWidth = 65;
        headerColumnStyle.alignment = TextAnchor.MiddleCenter;

        var rowStyle = new GUIStyle();
        rowStyle.fixedHeight = 25;
        rowStyle.alignment = TextAnchor.MiddleCenter;

        var dataFieldStyle = new GUIStyle(EditorStyles.miniButtonMid);
        dataFieldStyle.normal.background = Texture2D.grayTexture;
        dataFieldStyle.onNormal.background = Texture2D.whiteTexture;

        for (int row = 0; row < shapeDataInstance.rows; row++)
        {
            EditorGUILayout.BeginHorizontal(headerColumnStyle);
            for (int column = 0; column < shapeDataInstance.columns; column++)
            {
                EditorGUILayout.BeginHorizontal(rowStyle);
                var data = EditorGUILayout.Toggle(shapeDataInstance.board[row].column[column], dataFieldStyle);
                shapeDataInstance.board[row].column[column] = data;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

#endif
