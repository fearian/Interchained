using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Serialization;

public class SaveLoad : MonoBehaviour
{
    [SerializeField] private TMP_InputField puzzleNameInput;
    [SerializeField] private Levels levelsList;
    private BoardData1D<int> _boardData;
    
    public BoardData1D<int> SaveGame(HexGrid hexGrid)
    {
        _boardData = ToBoardData1D(hexGrid);
        levelsList.AddLevel(_boardData);
        return _boardData;
    }

    public BoardData1D<int> ToBoardData1D(HexGrid hexGrid)
    {
        int l = hexGrid.GetGridArray().GetLength(0);
        Debug.Log($"Loader: Getting hexGrid array length of {l}");
        _boardData = new BoardData1D<int>(l, l, puzzleNameInput.text);
        TileData[,] tileData = hexGrid.GetGridArray();
        Debug.Log($"Loader: Attempting to load tileData[{tileData.GetLength(0)},{tileData.GetLength(1)}] " +
                  $"into _boardData[{_boardData.Get_X_Length},{_boardData.Get_Y_Length}]");
        for (int x = 0; x < l; x++)
        {
            for (int y = 0; y < l; y++)
            {
                if (tileData[y, x] == null)
                {
                    _boardData[x, y] = 0;
                }
                else
                {
                    _boardData[x, y] = tileData[y, x].value;
                }
            }
        }

        return _boardData;
    }

    public BoardData1D<int> LoadLevel(string name)
    {
        if (name == "") return null;
        _boardData = levelsList.GetLevel(name);
        Debug.Log($"print this linear array value {_boardData[2,3]}");
        return _boardData;
    }

    public void CopyToClipboard(HexGrid hexGrid)
    {
        _boardData = ToBoardData1D(hexGrid);
        string json = FormatJson(_boardData);
        GUIUtility.systemCopyBuffer = FormatForClipboard(json);
    }

    private string FormatJson(BoardData1D<int> boardData)
    {
        string json = JsonUtility.ToJson(boardData, true);
        Debug.Log(json);
        
        return json;
    }

    private string FormatForClipboard(string input)
    {
        // Replace \n with actual newline characters and \t with tabs
        string formattedText = input.Replace("\\n", "\n").Replace("\\t", "\t");
        return formattedText;
    }

    public void LoadGame(string json)
    {
        
        Debug.LogWarning("Not Implemented Loading");
    }

    public void PasteFromClipboard()
    {
        string json = GUIUtility.systemCopyBuffer;
        LoadGame(json);
    }
    
    /*
    public void ArrayTest()
    {
        int n = 3, m = 3;
        int[] array = new int[100];
     
        // Initialising a 2-d array
        int [,]grid = {{1, 2, 3},
            {4, 5, 6},
            {7, 8, 9}};
     
        // storing elements in 1-d array
        int i, j, k = 0;
        for (i = 0; i < n; i++)
        {
            for (j = 0; j < m; j++)
            {
                k = i * m + j;
                array[k] = grid[i, j];
                k++;
            }
        }
     
        // displaying elements in 1-d array
        string result = "";
        for (i = 0; i < n; i++)
        {
            for (j = 0; j < m; j++)
                result += array[i * m + j] + " ";
            result += "\n";
        }
        Debug.Log(result);
        // This code is contributed by nitin mittal
    }*/
}

/*
[Serializable]
public class BoardData
{
    public int boardSize;
    public string boardState;
    public string Name;
    public string Description;

    public int[,] boardValues;
    public bool[,] boardLoop;

    public Linear2DArray<int> linearBoardValues;
    //public bool[,] locked;
    // hey maybe these should all be (short)bits.
    // 101010 cell value
    // 1010 loop/locked/valid/null
    // 6 bits to play with

    public BoardData(TileData[,] tileData, string puzzleName = "Unnamed Puzzle", string puzzleDescription = "")
    {
        Name = puzzleName;
        Description = puzzleDescription;
        boardSize = (tileData.GetLength(0) - 1) / 2;

        int arraySize = tileData.GetLength(0);
        boardValues = new int[arraySize, arraySize];
        linearBoardValues = new Linear2DArray<int>(arraySize, arraySize);

        boardState = "\n";
        for (int x = 0; x < arraySize; x++)
        {
            boardState += "\t[";
            string[] row = new string[arraySize];
            
            for (int y = 0; y < arraySize; y++)
            {
                if (tileData[x, y] == null)
                {
                    row[y] = "n";
                    boardValues[x, y] = 0;
                }
                else
                {
                    string v = tileData[x, y].value.ToString();
                    string l = tileData[x, y].isOnLoop ? "l" : "";
                    row[y] = v + l;
                    boardValues[x, y] = tileData[x, y].value;
                }
            }

            for (int i = 0; i < row.Length - 1; i++)
            {
                boardState += row[i] + ", ";
            }

            boardState += row[row.Length - 1] + "]\n";
        }

        boardState += "\t";
        //Debug.Log(boardState);

        linearBoardValues[2, 3] = boardValues[2, 3];
    }
}

[Serializable]
public class Linear2DArray<T> where T : struct { public int x, y; public T[] LinearArray;

    public T this[int x, int y]
    {
        get => LinearArray[y * this.x + x];
        set => LinearArray[y * this.x + x] = value;
    }

    public Linear2DArray(int x, int y)
    {
        this.x = x;
        this.y = y;
        LinearArray = new T[x * y];
    }

    public int Get_X_Length => x;
    public int Get_Y_Length => y;
    public int Length => LinearArray.Length;
}
*/
[Serializable]
public class BoardData1D<T> where T : struct
{
    public int x, y;
    public T[] LinearArray;
    public int BoardSize => (LinearArray.Length - 1) / 2;
    public string Name;
    public string Description;
    
    // 2D to 1D Array
    public T this[int x, int y]
    {
        get => LinearArray[y * this.x + x];
        set => LinearArray[y * this.x + x] = value;
    }

    public BoardData1D(int x, int y, string puzzleName = "Unnamed Puzzle", string puzzleDescription = "")
    {
        this.x = x;
        this.y = y;
        LinearArray = new T[x * y];
        Name = puzzleName;
        Description = puzzleDescription;
    }

    public int Get_X_Length => x;
    public int Get_Y_Length => y;
    public int Length => LinearArray.Length;
}

