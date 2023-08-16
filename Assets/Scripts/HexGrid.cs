using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DebugUtils;
using Color = UnityEngine.Color;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class HexGrid
{
    [SerializeField][Range(2, 7)] private int GridRadius = 3;
    private TileData[,] gridArray;
    [SerializeField] private TileData tileObject;
    private List<Hex> ValidHexes;
    
    
    private TextMeshPro[,] debugTextArray;

    public TileData[,] GetGridArray()
    {
        return gridArray;
    }

    public HexGrid(int GridRadius, TileData tileObject)
    {
        this.GridRadius = GridRadius;
        this.tileObject = tileObject;
        this.ValidHexes = new List<Hex>(Hex.Spiral(Hex.zero, 1, GridRadius));

        int arraySize = GridRadius * 2 + 1;
        gridArray = new TileData[arraySize, arraySize];
        debugTextArray = new TextMeshPro[arraySize, arraySize];

        
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                
                Hex hex = new Hex(y - GridRadius, x - GridRadius);
                if (ValidHexes.Contains(hex))
                {
                    gridArray[x, y] = CreateTile(y - GridRadius, x - GridRadius);
                }
                else
                {
                    DrawDebugHex(hex.ToWorld());
                    gridArray[x, y] = null;
                }
            }
        }

        //DrawDebugCoords();
    }

    private TileData CreateTile(int q, int r)
    {
        Hex hex = new Hex(q, r);
        Vector3 pos = hex.ToWorld();
        
        TileData tile = GameObject.Instantiate<TileData>(tileObject, pos, Quaternion.Euler(0,0,0), null);

        return tile;
    }

    public TileData GetTile(Hex hex)
    {
        int x, y;
        ToArray(hex, out x, out y);
        if (InGridArray(x,y)) return gridArray[x, y];
        else return null;
    }

    public bool InGridArray(int x, int y)
    {
        return x >= 0 
            && y >= 0 
            && x < gridArray.GetLength(0) 
            && y < gridArray.GetLength(1);
    }
    
    public void ToArray(Hex hex, out int x, out int y)
    {
        x = hex.r + GridRadius;
        y = hex.q + GridRadius;
        if (!ValidHexes.Contains(hex))
        {
            //Debug.LogWarning($"ToArray: invalid board hex {hex}, array element null!");
        }
    }

    public Hex ToHex(int x, int y)
    {
        return new Hex(y - GridRadius, x - GridRadius);
    }

    public void DrawDebugCoords()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (gridArray[x, y] != null)
                {
                    TileData tile = gridArray[x, y];
                    Vector3 pos = tile.hex.ToWorld();
                    debugTextArray[x,y] = CreateWorldText($"{tile.hex.q},{tile.hex.r}", null, pos + new Vector3(0,0.5f, 0), 3);
                    DrawDebugHex(pos);
                }
            }
        }
    }

    public void ClearBoard()
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (gridArray[x, y] != null)
                {
                    gridArray[y, x].ClearTile();
                }
            }
        }
    }

    public void SetBoard(BoardData1D<int> boardState)
    {
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                if (gridArray[x, y] != null)
                {
                    // cant read boardValues because it's null
                    // it is null, because 2d arrays are not serialised
                    gridArray[y, x].SetValue(boardState[y, x]);
                    //if (boardState.boardLoop[y, x] == true) gridArray[y, x].ToggleIsLoop();
                }
            }
        }
    }
}

public static class DebugUtils
{
    public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3),
        int fontSize = 30)
    {
        GameObject gameObject = new GameObject("Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.Rotate(new Vector3(90,0,0));
        TextMeshPro textMeshPro = gameObject.GetComponent<TextMeshPro>();
        textMeshPro.text = text;
        textMeshPro.fontSize = fontSize;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = Color.green;
        
        return textMeshPro;
    }
    
    public static void DrawDebugHex(Vector3 pos, float time = 60f)
    {
        Vector3 top = new Vector3(Hex.R_INV.x, 0, Hex.R_INV.y);
        Vector3 corner = new Vector3(Hex.Q_INV.x, 0, Hex.Q_INV.y);

        Debug.DrawLine(pos - corner, pos + top, Color.green, time);
        Debug.DrawLine(pos + top, pos + new Vector3(corner.x, 0, -corner.z), Color.green, time);
        Debug.DrawLine(pos + new Vector3(corner.x, 0, -corner.z), pos + corner, Color.green, time);
        Debug.DrawLine(pos + corner, pos - top, Color.green, time);
        Debug.DrawLine(pos - top, pos + new Vector3(-corner.x, 0, corner.z), Color.green, time);
        Debug.DrawLine(pos + new Vector3(-corner.x, 0, corner.z), pos - corner, Color.green, time);
    }
}
