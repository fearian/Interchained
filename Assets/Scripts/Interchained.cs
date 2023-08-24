using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;

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
    [SerializeField] public Toggle createModeToggle;
    public bool inCreateMode => createModeToggle.isOn;

    public UnityEvent onLevelSaved;
    public UnityEvent onLevelLoaded;

    private HexGrid hexGrid;

    private List<Hex> invalidTiles = new List<Hex>();
    private List<Hex> incorrectLoops = new List<Hex>();

    private Vector3 mousePos;
    void Start()
    {
        hexGrid = new HexGrid(size, tileObject);
        _validator = new Validator(hexGrid);
    }

    private void Update()
    {
        mousePos = GetMouseWorldPosition();
        AcceptInput();
    }

    private void AcceptInput()
    {
        // Mouse input on tiles
        if (Input.GetMouseButtonDown(0)) CycleValue(true);
        if (Input.GetMouseButtonDown(1)) CycleValue(false);
        
        // Place tiles 1-9
        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1)) SetTile(1);
        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2)) SetTile(2);
        if (Input.GetKeyUp(KeyCode.Alpha3) || Input.GetKeyUp(KeyCode.Keypad3)) SetTile(3);
        if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.Keypad4)) SetTile(4);
        if (Input.GetKeyUp(KeyCode.Alpha5) || Input.GetKeyUp(KeyCode.Keypad5)) SetTile(5);
        if (Input.GetKeyUp(KeyCode.Alpha6) || Input.GetKeyUp(KeyCode.Keypad6)) SetTile(6);
        if (Input.GetKeyUp(KeyCode.Alpha7) || Input.GetKeyUp(KeyCode.Keypad7)) SetTile(7);
        if (Input.GetKeyUp(KeyCode.Alpha8) || Input.GetKeyUp(KeyCode.Keypad8)) SetTile(8);
        if (Input.GetKeyUp(KeyCode.Alpha9) || Input.GetKeyUp(KeyCode.Keypad9)) SetTile(9);
        
        // Clear tiles
        if (Input.GetKeyUp(KeyCode.Delete) || Input.GetKeyUp(KeyCode.Backspace)) ClearTile();
        
        // Mark the loop
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.L)) MarkAsLoop();
        
        // Create Mode only inputs
        if (inCreateMode)
        {
            if (Input.GetKeyUp(KeyCode.F)) ToggleLocked();
            if (Input.GetKeyUp(KeyCode.Alpha0) || Input.GetKeyUp(KeyCode.Keypad0)) SetBlocker();
        }
    }

    private Hex HexUnderCursor()
    {
        return Hex.FromWorld(mousePos);
    }

    private TileData TileUnderCursor()
    {
        return hexGrid.GetTile(HexUnderCursor());
    }

    private void CycleValue(bool cycleUp = true)
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;
        
        tile.SetValue(tile.Value + (cycleUp ? +1 : -1));
        
        
        ValidationPass(tile);
    }

    private void SetTile(int value)
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;

        if (tile.Value == value)
        {
            ClearTile();
        }
        else tile.SetValue(value);
        
        ValidationPass(tile);

        //Debug.Log($"Tile ({hex.q},{hex.r}) value ({tile.Value}, {(tile.IsInvalid ? "Invalid" : "Valid")}), in region ({tile.region})");
    }

    private void ClearTile()
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;

        if (tile.IsEmpty) tile.ToggleIsLoop(false);
        else tile.SetValue(0);
        //else tile.ClearTile(inCreateMode);
        
        ValidationPass(tile);
    }
    
    private void MarkAsLoop(){
        TileData tile = TileUnderCursor();
        if (tile == null) return;
        tile.ToggleIsLoop();

        ReCheckIncorrectLoops();
        
        // validate loop placement
        if (tile.IsOnLoop) ValidateIsOnLoop(tile.hex);
    }

    private void ToggleLocked()
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;
        
        tile.MarkLocked(!tile.IsLocked);
    }

    private void SetBlocker()
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;
        if (tile.IsEmpty || tile.IsBlocker)
        {
            tile.MarkBlocker(!tile.IsBlocker);
        }
    }

    public void ValidationPass(TileData tile)
    {
        // Recheck previously invalid tiles, in case the recent change has fixed them
        ReCheckInvalidTiles();
        
        // Validate numbers and gears
        if (tile.IsNumber) ValidateBySudoku(tile.hex);
        else ValidateGear(tile.hex);
    }

    public void CheckBoard()
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

    private void ReCheckIncorrectLoops()
    {
        if (incorrectLoops.Count == 0) return;

        TileData tile;
        List<Hex> markOkay = new List<Hex>();

        foreach (Hex hex in incorrectLoops)
        {
            tile = hexGrid.GetTile(hex);

            if (!tile.IsOnLoop)
            {
                markOkay.Add(hex);
                tile.MarkInvalid(false, true);
            }
            else
            {
                int sidesTouchingLoop = _validator.SidesTouchingLoop(hex);
                Debug.Log($"{tile.hex} IsOnLoop:{tile.IsOnLoop}, touching {sidesTouchingLoop} other loop tiles.");
                if (sidesTouchingLoop < 3)
                {
                    markOkay.Add(hex);
                    tile.MarkInvalid(false, true);
                }
            }
        }

        foreach (Hex validLoop in markOkay)
        {
            incorrectLoops.Remove(validLoop);
        }
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
            else if (tile.IsGear)
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

    private void ValidateIsOnLoop(Hex hex)
    {
        var incorrectLoop = _validator.IsTouchingLoopIncorrectly(hex);

        foreach (Hex loopCell in incorrectLoop)
        {
            hexGrid.GetTile(loopCell).MarkInvalid(true, true);
            incorrectLoops.Add(loopCell);
            Debug.DrawLine(hex.ToWorld(), loopCell.ToWorld(), Color.red, 1f);
        }
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
        puzzleInfoField.text = "<b>" + boardState.Name + "</b>" + "\n" + boardState.Description;
        
        onLevelLoaded.Invoke();
    }
}
