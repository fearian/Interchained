using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SaveLoad : MonoBehaviour
{
    [SerializeField] private TMP_InputField puzzleNameInput;
    [SerializeField] private TMP_InputField jsonInputField;
    [SerializeField] private Levels levelsList;
    private BoardData1D<int> _boardData;

    public BoardData1D<int> SaveGame(HexGrid hexGrid)
    {
        _boardData = ToBoardData1D(hexGrid);
        levelsList.AddLevel(_boardData);
        return _boardData;
    }

    // Bit cheatsheat:
    // Bits 0 - 5 are reserved for cell values up to 64
    // bits 6 - 9 are flags for: 6:loop, 7:locked, 8:hidden, 9:invalid
    // bits 10 - 31 unused so far

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
                    _boardData[x, y] = tileData[y, x].Value;
                    _boardData[x, y] |= (tileData[y, x].IsOnLoop ? 1 : 0) << 6;
                    _boardData[x, y] |= (tileData[y, x].IsLocked ? 1 : 0) << 7;
                    _boardData[x, y] |= (tileData[y, x].IsHidden ? 1 : 0) << 8;
                    _boardData[x, y] |= (tileData[y, x].IsInvalid ? 1 : 0) << 9;
                    Debug.Log(_boardData[x,y].ToBinaryString());
                }
            }
        }

        return _boardData;
    }

    public BoardData1D<int> LoadLevel(string name)
    {
        if (name == "") return null;
        _boardData = levelsList.GetLevel(name);
        return _boardData;
    }

    public void CopyToClipboard(HexGrid hexGrid)
    {
        _boardData = ToBoardData1D(hexGrid);
        string json = FormatJson(_boardData);
        jsonInputField.text = FormatForClipboard(json);
        //GUIUtility.systemCopyBuffer = FormatForClipboard(json);
    }

    private string FormatJson(BoardData1D<int> boardData)
    {
        string json = JsonUtility.ToJson(boardData, false);
        Debug.Log(json);

        return json;
    }

    private string FormatForClipboard(string input)
    {
        // Replace \n with actual newline characters and \t with tabs
        string formattedText = input.Replace("\\n", "\n").Replace("\\t", "\t");
        return formattedText;
    }

    public BoardData1D<int> PasteFromClipboard()
    {
        string json = jsonInputField.text;//GUIUtility.systemCopyBuffer;
        try
        {
            return JsonUtility.FromJson<BoardData1D<int>>(json);
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Pasted data not valid JSON, or: {exception}");
            return null;
        }
    }
}


[Serializable]
public class BoardData1D<T> where T : struct
{
    public int x, y;
    public T[] BoardState;
    public int BoardSize => (BoardState.Length - 1) / 2;
    public string Name;
    [TextArea(3, 140)]public string Description;
    
    // 2D to 1D Array
    public T this[int x, int y]
    {
        get => BoardState[y * this.x + x];
        set => BoardState[y * this.x + x] = value;
    }

    public BoardData1D(int x, int y, string puzzleName = "Unnamed Puzzle", string puzzleDescription = "")
    {
        this.x = x;
        this.y = y;
        BoardState = new T[x * y];
        Name = puzzleName;
        Description = puzzleDescription;
    }

    public int Get_X_Length => x;
    public int Get_Y_Length => y;
    public int Length => BoardState.Length;
}

