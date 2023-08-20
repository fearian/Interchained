using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class Interchained : MonoBehaviour
{
    [SerializeField][Range(3,9)]
    private int size = 7;
    [SerializeField]
    private TileData tileObject;
    [SerializeField] public BoardColors boardColorPalette;
    [SerializeField]
    private SaveLoad saveLoadHandler;

    private Validator _validator;
    
    [SerializeField] private TextMeshProUGUI puzzleInfoField;

    public UnityEvent onLevelSaved;
    public UnityEvent onLevelLoaded;

    private HexGrid hexGrid;

    private List<Hex> invalidTiles = new List<Hex>();

    private Vector3 mousePos;
    void Start()
    {
        hexGrid = new HexGrid(size, tileObject);
        _validator = new Validator(hexGrid);
    }

    private void Update()
    {
        mousePos = GetMouseWorldPosition();
        //DebugUtils.DrawDebugHex(new Vector3(mousePos.x, 0, mousePos.z), 0.3f);
        if (Input.GetMouseButtonDown(0)) CycleValues(true);
        if (Input.GetMouseButtonDown(1)) CycleValues(false);
        if (Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.KeypadEnter)) MarkAsLoop();
    }

    private void CycleValues(bool cycleUp = true)
    {
        Hex hex = Hex.FromWorld(mousePos); 
        DebugUtils.DrawDebugHex(hex.ToWorld(), 0.1f);
        TileData tile = hexGrid.GetTile(hex);
        if (tile == null) return;
        if (cycleUp) tile.SetValue(tile.Value + 1);
        else tile.SetValue(tile.Value - 1);
        ReCheckInvalidTiles();
        if (tile.IsNumber) ValidateBySudoku(hex);
        else ValidateGear(hex);
        //Debug.Log($"Tile ({hex.q},{hex.r}) value ({tile.Value}, {(tile.IsInvalid ? "Invalid" : "Valid")}), in region ({tile.region})");
    }

    public void CheckBaord()
    {
        if (IsSolved())
        {
            Debug.Log("U WON!!!");
        }
        else Debug.Log("NOT SOLVED");
    }

    public bool IsSolved()
    {
        if (invalidTiles.Count != 0) return false;
        bool allValid = true;
        foreach (Hex hex in hexGrid.ValidHexes)
        {
            if (hexGrid.GetTile(hex).IsEmpty) allValid = false;
            var axis = _validator.IsDuplicatedAlongAxis(hex);
            var region = _validator.IsDuplicatedInRegion(hex);
            if (axis.Count() != 0 || region.Count() != 0)
            {
                allValid = false;
                var combined = axis.Union(region);
                foreach (Hex cell in combined)
                {
                    hexGrid.GetTile(cell).MarkInvalid(true);
                    Debug.DrawLine(hex.ToWorld(), cell.ToWorld(), Color.red, 1f);
                    invalidTiles.Add(cell);
                }

            }

        }
        return allValid;
    }

    private void ReCheckInvalidTiles()
    {
        if (invalidTiles.Count == 0) return;

        TileData tile;
        List<Hex> markedAsValid = new List<Hex>();
        
        foreach (Hex hex in invalidTiles)
        {
            tile = hexGrid.GetTile(hex);

            if (tile.IsNumber)
            {
                if (!_validator.InvalidNumber(hex))
                {
                    markedAsValid.Add(hex);
                    tile.MarkInvalid(false);
                }
            }
            else
            {
                if (!_validator.InvalidGear(hex))
                {
                    markedAsValid.Add(hex);
                    tile.MarkInvalid(false);
                    Debug.Log("Happily marking gear as now valid");
                }
            }
        }

        foreach (Hex validHex in markedAsValid)
        {
            invalidTiles.Remove(validHex);
        }
    }

    private void ValidateBySudoku(Hex hex)
    {
        var axis = _validator.IsDuplicatedAlongAxis(hex);
        var region = _validator.IsDuplicatedInRegion(hex);

        var combined = axis.Union(region);
        foreach (Hex cell in combined)
        {
            hexGrid.GetTile(cell).MarkInvalid(true);
            Debug.DrawLine(hex.ToWorld(), cell.ToWorld(), Color.red, 1f);
            invalidTiles.Add(cell);
        }
    }

    private void ValidateGear(Hex hex)
    {
        if (hexGrid.GetTile(hex).IsNumber) return;

        var neighbours = _validator.GearIsStuckOnGear(hex);
        var region = _validator.IsDuplicatedInRegion(hex);

        var combined = neighbours.Union(region);
        foreach (Hex cell in combined)
        {
            hexGrid.GetTile(cell).MarkInvalid(true);
            Debug.DrawLine(hex.ToWorld(), cell.ToWorld(), Color.red, 1f);
            invalidTiles.Add(cell);
        }
    }

    private void MarkAsLoop(){
        Hex hex = Hex.FromWorld(mousePos); 
        DebugUtils.DrawDebugHex(hex.ToWorld(), 0.1f);
        TileData tile = hexGrid.GetTile(hex);
        if (tile == null) return;
        Debug.Log($"Enter Key Down @{hex.q},{hex.r}. Loop: {tile.IsOnLoop}");
        tile.ToggleIsLoop();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return worldPosition;
    }

    public void SaveBoardState()
    {
        saveLoadHandler.SaveGame(hexGrid);
        
        onLevelSaved.Invoke();
    }

    public void LoadBoardState(string name = "")
    {
        Debug.Log($"Loading {name}");
        BoardData1D<int> boardState = saveLoadHandler.LoadLevel(name);
        if (boardState == null)
        {
            Debug.Log("BAD BOARD STATE ABOORT");
            return;
        }
        hexGrid.ClearBoard();
        hexGrid.SetBoard(boardState);
        puzzleInfoField.text = "<b>" + boardState.Name + "</b>" + "\n" + boardState.Description;
        
        onLevelLoaded.Invoke();
    }

    public void CopyClipboard()
    {
        saveLoadHandler.CopyToClipboard(hexGrid);
    }

    public void LoadClipboard()
    {
        BoardData1D<int> boardState =saveLoadHandler.PasteFromClipboard();
        if (boardState == null)
        {
            Debug.Log("BAD BOARD STATE ABOORT");
            return;
        }
        hexGrid.ClearBoard();
        hexGrid.SetBoard(boardState);
        
        onLevelLoaded.Invoke();
    }
}
