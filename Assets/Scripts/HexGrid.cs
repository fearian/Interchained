using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static DebugUtils;
using Color = UnityEngine.Color;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public enum BoardRegions
{
    None = 0,
    North = 1,
    East = 2,
    South = 3,
    West = 4
}

public class HexGrid
{
    //[SerializeField][Range(2, 7)]
    public int GridRadius { get; private set; } = 3;
    public int GridDiameter => GridRadius * 2 + 1;
    private TileData[,] _gridArray;
    [SerializeField] private TileData _tileObject;
    public List<Hex> ValidHexes { get; private set; }
    private Hex[] _regionNorth;
    private Hex[] _regionEast;
    private Hex[] _regionSouth;
    private Hex[] _regionWest;
    public Hex[][] Regions { get; private set; }


    private TextMeshPro[,] _debugTextArray;

    public TileData[,] GetGridArray()
    {
        return _gridArray;
    }

    public HexGrid(int gridRadius, TileData tileObject)
    {
        this.GridRadius = gridRadius;
        this._tileObject = tileObject;
        this.ValidHexes = new List<Hex>(Hex.Spiral(Hex.zero, 1, gridRadius));
        StoreRegions();

        int arraySize = gridRadius * 2 + 1;
        _gridArray = new TileData[arraySize, arraySize];
        _debugTextArray = new TextMeshPro[arraySize, arraySize];

        
        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                
                Hex hex = new Hex(y - gridRadius, x - gridRadius);
                if (ValidHexes.Contains(hex))
                {
                    _gridArray[x, y] = CreateTile(y - gridRadius, x - gridRadius);
                }
                else
                {
                    //DrawDebugHex(hex.ToWorld());
                    _gridArray[x, y] = null;
                }
            }
        }

        //DrawDebugCoords();
    }

    private TileData CreateTile(int q, int r)
    {
        Hex hex = new Hex(q, r);
        Vector3 pos = hex.ToWorld();
        
        TileData tile = GameObject.Instantiate<TileData>(_tileObject, pos, Quaternion.Euler(0,0,0), null);
        tile.name = $"Tile {tile.hex}";
        tile.region = GetRegion(hex);
        return tile;
    }

    public TileData GetTile(Hex hex)
    {
        int x, y;
        ToArray(hex, out x, out y);
        if (InGridArray(x,y)) return _gridArray[x, y];
        else return null;
    }

    public bool InGridArray(int x, int y)
    {
        return x >= 0 
            && y >= 0 
            && x < _gridArray.GetLength(0) 
            && y < _gridArray.GetLength(1);
    }
    
    public void ToArray(Hex hex, out int x, out int y)
    {
        x = hex.r + GridRadius;
        y = hex.q + GridRadius;
        if (!ValidHexes.Contains(hex))
        {
            //Debug.LogWarning($"ToArray: invalid board hex {hex}, array element null!");
            return;
        }
    }

    public Hex ToHex(int x, int y)
    {
        return new Hex(y - GridRadius, x - GridRadius);
    }

    public void DrawDebugCoords()
    {
        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                if (_gridArray[x, y] != null)
                {
                    TileData tile = _gridArray[x, y];
                    Vector3 pos = tile.hex.ToWorld();
                    _debugTextArray[x,y] = CreateWorldText($"{tile.hex.q},{tile.hex.r}", null, pos + new Vector3(0,0.5f, 0), 3);
                    DrawDebugHex(pos);
                }
            }
        }
    }

    public void ClearBoard()
    {
        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                if (_gridArray[x, y] != null)
                {
                    _gridArray[y, x].ClearTile(true);
                }
            }
        }
    }

    public void SetBoard(BoardData1D<int> boardState)
    {
        for (int x = 0; x < _gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < _gridArray.GetLength(1); y++)
            {
                if (_gridArray[x, y] != null)
                {
                    // See Bit Cheat Sheet in SaveLoad.cs
                    _gridArray[x, y].SetValue(boardState[y, x] & 0b111111);
                    if (((boardState[y, x] >> 6) & 0b1) == 1) _gridArray[x, y].MarkForLoop(true);
                    if (((boardState[y, x] >> 7) & 0b1) == 1) _gridArray[x, y].MarkLocked(true);
                    //if (((boardState[y, x] >> 8) & 0b1) == 1) _gridArray[x, y].MarkHidden(true);
                    //Invalid tiles, when loaded, are not added to the tracking array for re-assesment!
                    //if (((boardState[y, x] >> 9) & 0b1) == 1) _gridArray[x, y].MarkInvalid(true);
                    if (((boardState[y, x] >> 10) & 0b1) == 1) _gridArray[x, y].MarkBlocker(true);
                }
            }
        }
    }

    public void StoreRegions()
    {
        //Inverse cone for North (1) South (4) regions
        _regionNorth = Hex.InvCone(Hex.zero, 1, 3).ToArray();
        _regionSouth = Hex.InvCone(Hex.zero, 4, 3).ToArray();
        //Bias cone for East (5) West (2) regions
        _regionEast =  Hex.BiasCone((Hex.zero + Hex.AXIAL_DIRECTIONS[0]), 5, 3).ToArray();
        _regionWest =  Hex.BiasCone((Hex.zero + Hex.AXIAL_DIRECTIONS[3]), 2, 3).ToArray();

        Regions = new Hex[4][];
        Regions[0] = _regionNorth;
        Regions[1] = _regionEast;
        Regions[2] = _regionSouth;
        Regions[3] = _regionWest;
        
        //DebugRegions(15f);

        void DebugRegions(float time)
        {
            Hex next = Hex.zero;
            Hex last = next;
            foreach (Hex hex in Regions[0])
            {
                next = hex;
                Debug.DrawLine(last.ToWorld(), next.ToWorld(), Color.red, time);
                last = next;
            }
            foreach (Hex hex in Regions[1])
            {
                next = hex;
                Debug.DrawLine(last.ToWorld(), next.ToWorld(), Color.green, time);
                last = next;
            }
            foreach (Hex hex in Regions[2])
            {
                next = hex;
                Debug.DrawLine(last.ToWorld(), next.ToWorld(), Color.blue, time);
                last = next;
            }
            foreach (Hex hex in Regions[3])
            {
                next = hex;
                Debug.DrawLine(last.ToWorld(), next.ToWorld(), Color.magenta, time);
                last = next;
            }
        }
    }

    public BoardRegions GetRegion(Hex hex)
    {
        if (Regions == null)
        {
            Debug.LogError("Board Regions not assigned in Board Manager/Hex Grid!");
            return 0;
        }
        
        int i = 1;
        foreach (Hex[] region in Regions)
        {
            if (region.Contains(hex))
            {
                return (BoardRegions)i;
            }
            i++;
        }
        Debug.LogWarning($"GetRegion: Hex {hex} not found in _regions.");
        return (BoardRegions)0;
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
