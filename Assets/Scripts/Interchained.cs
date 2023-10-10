using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;
using static HexMsg;

public class Interchained : MonoBehaviour
{
    [SerializeField][Range(3,9)]
    private int size = 7;
    [SerializeField] private TileData tileObject;
    [SerializeField] public BoardColors boardColorPalette;
    [SerializeField] private SaveLoad saveLoadHandler;
    [SerializeField] private LongPressDetection _longPressDetection;

    private Validator _validator;
    [SerializeField] private LoopDrawer _loopDrawer;
    
    [SerializeField] private TextMeshProUGUI puzzleInfoField;
    [SerializeField] public Toggle createModeToggle;
    public bool inCreateMode => createModeToggle.isOn;
    [SerializeField] private TileLabels tileLabelsNumeric;
    [SerializeField] private TileLabels tileLabelsAlpha;

    public UnityEvent onLevelSaved;
    public UnityEvent onLevelLoaded;

    private HexGrid hexGrid;

    private List<Hex> invalidTiles = new List<Hex>();
    private List<Hex> invalidLoopTiles = new List<Hex>();
    private List<TileData> possibleLoop = new List<TileData>();

    private Vector3 mousePos;
    void Start()
    {
        hexGrid = new HexGrid(size, tileObject);
        _validator = new Validator(hexGrid);
        _loopDrawer = _loopDrawer.Initialize(hexGrid);
        _longPressDetection.onLongPress.AddListener(LongPressReaction);
    }

    private void Update()
    {
        mousePos = GetMouseWorldPosition();
        AcceptInput();
    }

    #region Player Input
    // Take hardware inputs
    
    private void LongPressReaction()
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;
        tile.loopParticles.Play();
        MarkAsLoop();
    }

    private void AcceptInput()
    {
        // Mouse input on tiles
        if (_longPressDetection.isDetectingLongPress == false && Input.GetMouseButtonDown(0)) CycleValue(true);
        if (Input.GetMouseButtonDown(1)) CycleValue(false);
        
        // Place tiles 1-9
        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1)) SetTile(1);
        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2)) SetTile(2);
        if (Input.GetKeyUp(KeyCode.Alpha3) || Input.GetKeyUp(KeyCode.Keypad3) || Input.GetKeyUp(KeyCode.A)) SetTile(3);
        if (Input.GetKeyUp(KeyCode.Alpha4) || Input.GetKeyUp(KeyCode.Keypad4) || Input.GetKeyUp(KeyCode.B)) SetTile(4);
        if (Input.GetKeyUp(KeyCode.Alpha5) || Input.GetKeyUp(KeyCode.Keypad5) || Input.GetKeyUp(KeyCode.X)) SetTile(5);
        if (Input.GetKeyUp(KeyCode.Alpha6) || Input.GetKeyUp(KeyCode.Keypad6) || Input.GetKeyUp(KeyCode.Y)) SetTile(6);
        if (Input.GetKeyUp(KeyCode.Alpha7) || Input.GetKeyUp(KeyCode.Keypad7)) SetTile(7);
        if (Input.GetKeyUp(KeyCode.Alpha8) || Input.GetKeyUp(KeyCode.Keypad8)) SetTile(8);
        if (Input.GetKeyUp(KeyCode.Alpha9) || Input.GetKeyUp(KeyCode.Keypad9)) SetTile(9);
        
        // "Swap" pair digits on a tile
        if (Input.GetKeyUp(KeyCode.Space)) SwapTilePair();
        
        // Clear tiles
        if (Input.GetKeyUp(KeyCode.Delete) || Input.GetKeyUp(KeyCode.Backspace)) ClearTile();
        
        // Mark the loop
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.L)) MarkAsLoop();
        
        // Create Mode only inputs
        if (inCreateMode)
        {
            if (Input.GetKeyUp(KeyCode.F)) ToggleLocked();
            if (Input.GetKeyUp(KeyCode.Alpha0) || Input.GetKeyUp(KeyCode.Keypad0)) SetBlocker();
        }
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return worldPosition;
    }
    
    private Hex HexUnderCursor()
    {
        return Hex.FromWorld(mousePos);
    }
    
    private TileData TileUnderCursor()
    {
        return hexGrid.GetTile(HexUnderCursor());
    }

    #endregion
    
    #region Change Tile Values
    // give tiles a value, swap pair values, clear tiles

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
    }

    private void SwapTilePair()
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;

        if (tile.IsPaired)
        {
            bool tileWasLower = tile.IsLowerOfPair;
            TileData pair = tile.pairedTile;
            tile.SetValue( (tileWasLower) ? tile.Value + 1 : tile.Value - 1 );
            pair.SetValue( (tileWasLower) ? pair.Value - 1 : pair.Value + 1 );
            tile.SetPairedTile(pair);
            pair.SetPairedTile(tile);
            ValidationPass(tile, true, true, true, true);
            ValidationPass(pair, true, true, true, true);
        }
        else if (tile.IsGear)
        {
            tile.SetValue((tile.Value == 8) ? 9 : 8);
            ValidationPass(tile);
        }
        else if (!tile.IsPaired)
        {
            ValidationPass(tile, false, false, true, true, true);
        }
        else return;
    }

    private void ClearTile()
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;

        TileData pairedHack = null;
        if (tile.IsEmpty) tile.ToggleIsLoop(false);
        else
        {
            if (tile.IsPaired)
            {
                pairedHack = tile.pairedTile;
            }
            tile.SetValue(0);
        }
        
        ValidationPass(tile);
        
        if (pairedHack != null) ValidationPass(pairedHack);
    }
    
    #endregion

    #region Change Tile Status
    // Mark as on the loop, lock the tile, set as a blocker

    private void MarkAsLoop(){
        TileData tile = TileUnderCursor();
        if (tile == null) return;
        tile.ToggleIsLoop();

        ValidationPass(tile, true, false, false, true);
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

    #endregion

    #region Validate Tile Data

    public void ValidationPass(TileData tile, bool recheckInvalid = true, bool validatePlacement = true, 
        bool assignPairs = true, bool validateLoop = true, bool redrawLoop = true)
    {
        if (!(tile != null)) return;
        
        if (recheckInvalid) ReCheckInvalidTiles();
        
        if (validatePlacement) ValidatePlacement(tile);
        if (assignPairs) AssignPairs(tile);
        if (validateLoop) ValidateLoop(tile);
        
        if(redrawLoop) _loopDrawer.TryToDrawLoop(possibleLoop.ToArray(), tile);

        return;
        
        void ValidatePlacement(TileData tile)
        {
            if (tile.Value == 0) tile.MarkInvalid(false);
            else if (tile.IsNumber) ValidateBySudoku(tile.hex);
            else if (tile.IsGear) ValidateGear(tile.hex);
        }

        void AssignPairs(TileData tile)
        {
            if (tile.Value >= 1 && tile.Value <= 7) CheckForPair(tile);
        }

        void ValidateLoop(TileData tile)
        {
            ReCheckIncorrectLoops();

            if (tile.IsMarkedForLoop)
            {
                ValidateIsOnLoop(tile);
                if (tile.IsPaired && tile.pairedTile.IsMarkedForLoop) ValidateIsOnLoop(tile.pairedTile);
            }
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

            if (tile.IsEmpty)
            {
                markedAsValid.Add(hex);
                tile.MarkInvalid(false);
            }
            
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
                    //Debug.Log("Happily marking gear as now valid");
                }
            }
        }

        foreach (Hex validHex in markedAsValid)
        {
            invalidTiles.Remove(validHex);
            ValidateIsOnLoop(hexGrid.GetTile(validHex));
        }
    }

    private void ReCheckIncorrectLoops()
    {
        if (invalidLoopTiles.Count == 0) return;

        TileData tile;
        List<Hex> markOkay = new List<Hex>();

        foreach (Hex hex in invalidLoopTiles)
        {
            tile = hexGrid.GetTile(hex);

            if (!tile.IsMarkedForLoop || tile.IsPaired)
            {
                markOkay.Add(hex);
                tile.MarkInvalid(false, true);
            }
            else
            {
                int sidesTouchingLoop = _validator.SidesTouchingLoop(hex);
                //Debug.Log($"{tile.hex} IsOnLoop:{tile.IsMarkedForLoop}, touching {sidesTouchingLoop} other loop tiles.");
                if (sidesTouchingLoop < 3 && tile.IsPaired)
                {
                    markOkay.Add(hex);
                    tile.MarkInvalid(false, true);
                }
            }
        }

        foreach (Hex validLoop in markOkay)
        {
            invalidLoopTiles.Remove(validLoop);
        }
    }
    
    #region Validate Tile Values
    

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

    #endregion

    #region validate Tile Loop Status

    private void ValidateIsOnLoop(TileData tile)
    {
        if (!(tile != null)) return;
        
        bool msg = true;
        if (msg) ClearMsg(tile.hex);
        if (msg) AddMsg(tile.hex, "check.", true);

        bool passedChecks = true;

        if (!tile.IsMarkedForLoop)
        {
            if (possibleLoop.Contains(tile)) possibleLoop.Remove(tile);
            if (invalidLoopTiles.Contains(tile.hex) == true) invalidLoopTiles.Remove(tile.hex);
            if (msg) AddMsg(tile.hex, $"Not Loop!");
            Debug.LogWarning($"{tile} Validated for loop, but is not marked as loop!");
        }

        if (!tile.IsPaired || tile.IsInvalid || tile.IsEmpty)
        {
            if (msg) AddMsg(tile.hex, $"{((tile.IsPaired) ? "paired" : "")}" +
                                      $"{((tile.IsInvalid) ? "invalid" : "")}" +
                                      $"{((tile.IsEmpty) ? "empty" : "")}(X)");
            tile.MarkLoopAsIncorrect(true);
            if (invalidLoopTiles.Contains(tile.hex) == false) invalidLoopTiles.Add(tile.hex);
            if (possibleLoop.Contains(tile)) possibleLoop.Remove(tile);
            passedChecks = false;
        }
        
        var touchesLoopIncorrectly = _validator.IsTouchingLoopIncorrectly(tile.hex);

        foreach (Hex hex in touchesLoopIncorrectly)
        {
            if (msg) AddMsg(tile.hex, $"touching(X)", true);
            hexGrid.GetTile(hex).MarkLoopAsIncorrect(true);
            if (invalidLoopTiles.Contains(hex) == false) invalidLoopTiles.Add(hex);
            if (possibleLoop.Contains(tile)) possibleLoop.Remove(tile);
            passedChecks = false;
            Debug.DrawLine(tile.hex.ToWorld(), hex.ToWorld(), Color.red, 1f);
        }
        
        if (passedChecks)
        {
            if (msg) AddMsg(tile.hex, $"(O)", true);
            tile.MarkLoopAsIncorrect(false);
            if (invalidLoopTiles.Contains(tile.hex) == true) invalidLoopTiles.Remove(tile.hex);
            if (possibleLoop.Contains(tile) == false) possibleLoop.Add(tile);
        }
    }
    
    
    private void CheckForPair(TileData tile, bool msg = false)
    {
        if (msg) ClearMsg();

        var potentialPairs = _validator.NeighboursCanPair(tile);
        if (potentialPairs.Count() <= 0) return;
        
        TileData firstChoice = null;
        int bestScore = 0;
        foreach (TileData other in potentialPairs)
        {
            bool otherIsMyPair = false;
            if (other.IsPaired)
            {
                otherIsMyPair = (other.pairedTile == tile);
            }
            
            // Single Tile Response
            if (potentialPairs.Count() == 1)
            {
                if (other.IsPaired)
                {
                    if (other.IsMarkedForLoop &&
                        (tile.IsMarkedForLoop == false || other.pairedTile.IsMarkedForLoop))
                        break;
                    else if (other.IsMarkedForLoop && tile.IsMarkedForLoop)
                    {
                        firstChoice = other;
                        break;
                    }
                }
                else
                {
                    firstChoice = other;
                    break;
                }
            }

            //Multi Tile evaluation
            int score = 10;

            if (other.IsInvalid) score--;
            else score++;
            if (tile.IsMarkedForLoop) score++;
            if (other.IsMarkedForLoop)
            {
                if (tile.IsMarkedForLoop) score++;
                else score--;
            }
            if (other.IsPaired && !otherIsMyPair)
            {
                score--;
                if (other.pairedTile.IsMarkedForLoop)
                {
                    score--;
                    if (other.pairedTile.IsOnLoopIncorrectly) score++;
                    else score--;
                }
                else
                {
                    score++;
                    if (other.pairedTile.IsInvalid) score++;
                    if (other.pairedTile.IsInvalid && !tile.IsInvalid) score++;
                }
            }
            if (msg) AddMsg(other.hex, $":{score}!", true);

            if (score >= bestScore)
            {
                bestScore = score;
                firstChoice = other;
            }
        }

        if (firstChoice == null)
        {
            if (msg) AddMsg(tile.hex, "N", true);
            return;
        }
        
        if (tile.IsPaired) tile.RemovePair();
        if (firstChoice.IsPaired) firstChoice.RemovePair();
        tile.SetPairedTile(firstChoice);
        firstChoice.SetPairedTile(tile);
    }

    private void MarkPossibleLoop(TileData tile)
    {
        if (possibleLoop.Contains(tile)) return;
        possibleLoop.Add(tile);
    }

    private void NotPossibleLoop(TileData tile)
    {
        if (possibleLoop.Contains(tile))
        {
            possibleLoop.Remove(tile);
        }
    }

    private void ResetPossibleLoop()
    {
        possibleLoop.Clear();
        possibleLoop = new List<TileData>();
    }
    

    #endregion
    
    #region Validate Board State
    public void CheckBoard()
    {
        foreach (TileData tile in hexGrid.GetGridArray())
        {
            if (tile != null) ValidationPass(tile, false);
        }
        _loopDrawer.TryToDrawLoop(possibleLoop.ToArray());
        
        if (IsSolved())
        {
            Debug.Log("U WON!!!");
        }
        else Debug.Log("NOT SOLVED");
    }

    private IEnumerator WaitForKeyPress(KeyCode key)
    {
        bool waitForInput = true;

        while (waitForInput)
        {
            if (Input.GetKeyDown(key))
            {
                waitForInput = false;
            }

            yield return null;
        }
    }

    public bool IsSolved()
    {
        if (invalidTiles.Count != 0) return false;
        else return true;
    }
    
    #endregion
    
    #endregion

    #region Save and Load

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
        ClearGameState();
        hexGrid.SetBoard(boardState);
        puzzleInfoField.text = "<b>" + boardState.Name + "</b>" + "\n" + boardState.Description;
        
        CheckBoard();
        
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
        ClearGameState();
        hexGrid.SetBoard(boardState);
        puzzleInfoField.text = "<b>" + boardState.Name + "</b>" + "\n" + boardState.Description;

        CheckBoard();
        
        onLevelLoaded.Invoke();
    }

    private void ClearGameState()
    {
        _loopDrawer.ClearLoop();
        invalidLoopTiles.Clear();
        invalidLoopTiles = new List<Hex>();
        invalidTiles.Clear();
        invalidTiles = new List<Hex>();
        possibleLoop.Clear();
        possibleLoop = new List<TileData>();
        ClearMsg();
        hexGrid.ClearBoard();
    }

    private bool useNumbers = false;
    public void ToggleTileLabels()
    {
        foreach (TileData tile in hexGrid.GetGridArray())
        {
            if (!(tile != null)) continue;
            TileLabels labelVisuals = (useNumbers ? tileLabelsNumeric : tileLabelsAlpha);
            if (!(labelVisuals != null)) return;

            TileVisuals visuals = tile.gameObject.GetComponent<TileVisuals>();
            visuals.SwapLabelVisuals(labelVisuals);
            useNumbers = !useNumbers;
        }
    }
    
    #endregion

    #region Debugging Logs
    #if UNITY_EDITOR
    
    private void LogTileStatus(TileData tile, bool logValue = true, bool logPair = false, bool logLoop = false, bool LogGear = false)
    {
        string message = $"<b><color=green>Tile Status:</b></color> ({tile.hex.q},{tile.hex.r}) \n";
        if (logValue) message += $"value ({tile.Value}, {(tile.IsInvalid ? "Invalid" : "Valid")}), in region ({tile.region}) \n";
        if (logPair) message += $"Tile is {((tile.IsPaired) ? "Paired" : "Single")}. I am the {((tile.IsLowerOfPair) ? "Lower" : "Higher")} of the pair.";
        if (logLoop) message += $"Tile {((tile.IsMarkedForLoop) ? "is" : "is not")} marked as on loop. ({((tile.IsOnLoopIncorrectly) ? "incorrectly" : "correctly")})." +
                                $" and is listed under {((invalidLoopTiles.Contains(tile.hex)) ? "\"Incorrect Loops\"," : "")} {((possibleLoop.Contains(tile)) ? "\"Possible Loop\"," : "")}.";
        Debug.Log(message);
    }

    private void OnDrawGizmos()
    {
        DrawMsg();
    }
    
    #endif
    #endregion
}

public static class HexMsg
{
    public static Dictionary<Hex, string> _tileMsg = new Dictionary<Hex, string>();
    private static GUIStyle msgStyle = new GUIStyle();

    public static void AddMsg(Hex hex, string msg, bool additive = false)
    {
        if (_tileMsg.ContainsKey(hex))
        {
            if (additive) _tileMsg[hex] += "/"+msg;
            else _tileMsg[hex] = msg;
        }
        else
        {
            _tileMsg.Add(hex, msg);
        }
    }

    public static void ClearMsg(Hex hex)
    {
        if (_tileMsg.ContainsKey(hex))
        {
            _tileMsg.Remove(hex);
        }
    }

    public static void ClearMsg()
    {
        _tileMsg.Clear();
    }

    #if UNITY_EDITOR
    public static void DrawMsg()
    {
        msgStyle.fontSize = 18;
        msgStyle.richText = true;
        msgStyle.alignment = TextAnchor.MiddleCenter;
        msgStyle.wordWrap = true;
        msgStyle.fixedWidth = 100f;
        foreach (var msg in _tileMsg)
        {
            UnityEditor.Handles.Label(msg.Key.ToWorld() + new Vector3(0,0,0.25f), msg.Value, msgStyle);
        }
    }
    #endif

}
