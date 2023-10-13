using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
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

    private List<TileData> invalidTiles = new List<TileData>();

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
        
        EvaluationCycle(tile);
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
        
        EvaluationCycle(tile);
    }

    // TODO: revisit
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
            EvaluationCycle(tile);
            EvaluationCycle(pair);
        }
        else if (tile.IsGear)
        {
            tile.SetValue((tile.Value == 8) ? 9 : 8);
            EvaluationCycle(tile);
        }
        else if (!tile.IsPaired)
        {
            EvaluationCycle(tile);
        }
        else return;
    }

    private void ClearTile()
    {
        TileData tile = TileUnderCursor();
        if (tile == null) return;

        // validate a potential pair that misses it's partner being deleted
        TileData pairedTile = null;
        
        ClearMsg(tile.hex);

        if (tile.IsEmpty && tile.IsMarkedForLoop) tile.MarkForLoop(false);
        else
        {
            if (tile.IsPaired) pairedTile = tile.pairedTile;
            
            tile.SetValue(0);
        }

        EvaluationCycle(tile);
        if (pairedTile != null) EvaluationCycle(pairedTile);
    }
    
    private void MarkAsLoop(){
        TileData tile = TileUnderCursor();
        if (tile == null) return;
        tile.ToggleIsLoop();

        EvaluationCycle(tile);
    }
    
    #endregion

    #region Change Tile Status

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

    #region Evaluation Cycle

    public void EvaluationCycle(TileData tile)
    {
        if (!(tile != null)) return;
        
        ReevaluateInvalidTiles();
        
        ValidatePlacement(tile);
        
        AssignPairs(tile);
        
        ValidateLoop(tile);
        
        //RefreshDirtyTileVisuals();

        RedrawLoop(tile);
    }

    private void ReevaluateInvalidTiles()
    {
        if (invalidTiles.Count == 0) return;
        
        List<TileData> PassesPlacementEval = new List<TileData>();
        
        // Placement Evaluation
        foreach (var tile in invalidTiles)
        {
            if (tile.IsEmpty)
            {
                PassesPlacementEval.Add(tile);
            }
            
            if (tile.IsNumber)
            {
                if (_validator.InvalidNumber(tile) == false)
                {
                    PassesPlacementEval.Add(tile);
                }
            }
            else if (tile.IsGear)
            {
                if (_validator.InvalidGear(tile) == false)
                {
                    PassesPlacementEval.Add(tile);
                }
            }
            
        }

        // loop Evaluation
        foreach (var passingTile in PassesPlacementEval)
        {
            // release any so-far valid tile not on loop
            if (passingTile.IsMarkedForLoop == false)
            {
                passingTile.MarkAsValid();
                invalidTiles.Remove(passingTile);
            }
            // test if a tile is on loop correctly
            else
            {
                ValidateIsOnLoop(passingTile);
            }
        }
    }
    
    private void ValidatePlacement(TileData tile)
    {
        if (tile.IsEmpty)
        {
            tile.MarkAsValid();
        }
        else if (tile.IsNumber)
        {
            // Validate By Sudoku
            var axis = _validator.IsDuplicatedAlongAxis(tile);
            var region = _validator.IsDuplicatedInRegion(tile);

            var combined = axis.Union(region);
            foreach (var invalidTile in combined)
            {
                invalidTile.MarkAsInvalid(true);
                Debug.DrawLine(tile.hex.ToWorld(), invalidTile.hex.ToWorld(), Color.red, 1f);
                invalidTiles.Add(invalidTile);
            }
        }
        else if (tile.IsGear)
        {
            // Validate by Adjacent
            var neighbours = _validator.GearIsStuckOnGear(tile);
            var region = _validator.IsDuplicatedInRegion(tile);

            var combined = neighbours.Union(region);
            foreach (var invalidTile in combined)
            {
                invalidTile.MarkAsInvalid(true);
                Debug.DrawLine(tile.hex.ToWorld(), invalidTile.hex.ToWorld(), Color.red, 1f);
                invalidTiles.Add(invalidTile);
            }
        }
    }
    
    private void AssignPairs(TileData tile)
    {
        if (!(tile.Value >= 1 && tile.Value <= 6)) return;

        var foundPair = CheckForPair(tile);

        if (foundPair == null) return;
        
        TileData thisOldPair = null;
        TileData otherOldPair = null;
        if (tile.IsPaired)
        {
            thisOldPair = tile.pairedTile;
            tile.RemovePair();
        }
        if (foundPair.IsPaired)
        {
            otherOldPair  = foundPair.pairedTile;
            foundPair.RemovePair();
        }
        tile.SetPairedTile(foundPair);
        foundPair.SetPairedTile(tile);
        
        //if (thisOldPair != null) CheckForPair(thisOldPair);
        //if (otherOldPair != null) CheckForPair(otherOldPair);
    }
    
    private TileData CheckForPair(TileData thisTile, bool msg = true)
    {
        if (msg) ClearMsg();
        
        var potentialPairs = _validator.NeighboursCanPair(thisTile);
        if (potentialPairs.Count() <= 0) return null;

        TileData bestPair = null;
        int[] existingScores = new int[potentialPairs.Count()];
        int[] replacingScores = new int[potentialPairs.Count()];

        int i = 0;
        foreach (var otherTile in potentialPairs)
        {
            if (otherTile.IsPaired) existingScores[i] = ScorePair(otherTile, otherTile.pairedTile);
            else existingScores[i] = ScorePair(otherTile);
            if (msg) AddMsg(otherTile.hex, $"{i}:({existingScores[i]})", true);
            i++;
        }

        i = 0;
        foreach (var otherTile in potentialPairs)
        {
            replacingScores[i] = ScorePair(otherTile, thisTile);
            if (msg) AddMsg(otherTile.hex, $" (vs:{replacingScores[i]})", true);
            i++;
        }

        int bestScore = -99;
        for (int s = 0; s < existingScores.Length; s++)
        {
            string tracking = $"checking [{s}]";
            if (existingScores[s] >= bestScore)
            {
                tracking += $" existingScore:{existingScores[s]} >= bestScore:{bestScore}";

                if (existingScores[s] < replacingScores[s])
                {
                    tracking += $" existingScore:{existingScores[s]} < replacingScore:{replacingScores[s]}";
                    bestPair = potentialPairs.ToArray()[s];
                    bestScore = existingScores[s];
                    if (msg) AddMsg(thisTile.hex, $"{s} best", true);
                }
                else if (existingScores[s] == replacingScores[s])
                {
                    tracking += $" existingScore:{existingScores[s]} == replacingScore:{replacingScores[s]}";
                    TileData result = potentialPairs.ToArray()[s];
                    if (!result.IsPaired)
                    {
                        bestPair = potentialPairs.ToArray()[s];
                        bestScore = existingScores[s];
                        if (msg) AddMsg(thisTile.hex, $"{s} best", true);
                    }
                }
            }

            Debug.Log(tracking);
        }

        return bestPair;

        int ScorePair(TileData tileA, TileData tileB = null)
        {
            int score = 0;
            
            if (tileA.IsMarkedForLoop) score += 2;
            if (tileA.IsInvalid) score--;
            if (tileB != null)
            {
                if (tileA.pairedTile == tileB) score += 3;
                if (tileB.IsMarkedForLoop) score += 2;
                if (tileA.IsMarkedForLoop && tileB.IsMarkedForLoop) score +=2;
                if (tileB.IsInvalid) score--;
            }

            return score;
        }
        
        /*
        TileData firstChoice = null;
        int lowestOtherScore = 99;
        int againstNewScore = -99;

        i = 0;
        foreach (var otherTile in potentialPairs)
        {
            i++;
            int otherScore = 0; // stability score of existing other pair
            int newScore = 0; // stability score of new pair with other
            
            bool otherIsMyPair = (otherTile.IsPaired && otherTile.pairedTile == thisTile);

            if (otherTile.IsPaired && otherIsMyPair)
            {
                if (otherTile.IsMarkedForLoop){newScore+=2;}
                if (otherTile.IsMarkedForLoop && thisTile.IsMarkedForLoop) newScore +=1;
                
                if (msg) AddMsg(otherTile.hex, $"{i}:paired", true);
            }
            else if (otherTile.IsPaired && !otherIsMyPair)
            {
                TileData othersPairedTile = otherTile.pairedTile;

                otherScore += 2;
                if (otherTile.IsMarkedForLoop) otherScore += 2;
                if (othersPairedTile.IsMarkedForLoop) otherScore += 2;
                if (otherTile.IsMarkedForLoop && othersPairedTile.IsMarkedForLoop) otherScore +=1;

                if (otherTile.IsInvalid) otherScore--;
                if (othersPairedTile.IsInvalid && !thisTile.IsInvalid) otherScore--;
                
                if (msg) AddMsg(othersPairedTile.hex, $"{i}:({otherScore})", true);
            }
            else if (otherTile.IsPaired == false)
            {
                if (otherTile.IsMarkedForLoop) newScore+=2;
                if (otherTile.IsInvalid) newScore--;
            }

            if (thisTile.IsMarkedForLoop) newScore += 2;
            if (thisTile.IsInvalid) newScore--;
            
            if (msg) AddMsg(otherTile.hex, $"{i}:({otherScore}) vs ({newScore}):0", true);

            if (otherScore <= lowestOtherScore)
            {
                lowestOtherScore = otherScore;
                firstChoice = otherTile;
                againstNewScore = newScore;
            }
        }
        
        if (msg) AddMsg(thisTile.hex, $"n:{againstNewScore} vs ({lowestOtherScore}):0", true);
        
        if ((lowestOtherScore - againstNewScore) > 3 ) return null;

        return firstChoice;
        */
    }
    
    void ValidateLoop(TileData tile)
    {
        //TODO: Recheck incorrect loops
            
        if (tile.IsMarkedForLoop)
        {
            ValidateIsOnLoop(tile);
            if (tile.IsPaired && tile.pairedTile.IsMarkedForLoop) ValidateIsOnLoop(tile.pairedTile);
        }
    }
    #endregion

    #region validate Tile Loop Status

    private void ValidateIsOnLoop(TileData tile)
    {
        if (!(tile != null)) return;
        
        bool msg = false;
        if (msg) ClearMsg();
        if (msg) AddMsg(tile.hex, "check", false);

        bool passedChecks = true;

        if (!tile.IsMarkedForLoop) return;

        /*if (!tile.IsPaired || tile.IsEmpty)
        {
            if (msg) AddMsg(tile.hex, $"(X:{((!tile.IsPaired) ? "!p" : "")}" +
                                      $"{((tile.IsEmpty) ? ",0" : "")})");
            tile.MarkAsInvalid();
            passedChecks = false;
        }*/
        
        var touchesLoopIncorrectly = _validator.IsTouchingLoopIncorrectly(tile);

        foreach (var incorrectTile in touchesLoopIncorrectly)
        {
            incorrectTile.MarkAsInvalid();
            passedChecks = false;
            
            if (msg) AddMsg(incorrectTile.hex, $"(X:∴)", false);
            Debug.DrawLine(tile.hex.ToWorld(), incorrectTile.hex.ToWorld(), Color.red, 1.5f);
        }
        
        if (passedChecks)
        {
            if (msg) AddMsg(tile.hex, $"(O)", false);
            tile.MarkAsValid();
        }
    }

    public void RedrawLoop(TileData lastTile = null)
    {
        List<TileData> validLoopTiles = new List<TileData>();
        
        foreach (var hex in hexGrid.ValidHexes)
        {
            TileData tile = hexGrid.GetTile(hex);
            if (tile.IsMarkedForLoop && !tile.IsInvalid) validLoopTiles.Add(tile);
        }

        if (validLoopTiles.Count <= 0) return;
        
        _loopDrawer.TryToDrawLoop(validLoopTiles.ToArray());
    }

    #endregion
    
    #region Validate Board State
    public void CheckBoard()
    {
        foreach (TileData tile in hexGrid.GetGridArray())
        {
            if (tile != null) EvaluationCycle(tile);
        }

        RedrawLoop();
        
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
        invalidTiles.Clear();
        invalidTiles = new List<TileData>();
        ClearMsg();
        hexGrid.ClearBoard();
    }

    private bool useNumbers = true;
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
        if (logLoop) message += $"Tile {((tile.IsMarkedForLoop) ? "is" : "is not")} marked as on loop.";
        Debug.Log(message);
    }

    private void OnDrawGizmos()
    {
        DrawMsg();
    }
    
    #endif
    #endregion
}