using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
        foreach (var cell in _validator.IsDuplicatedAlongAxis(hex))
        {
            DebugUtils.DrawDebugHex(cell.ToWorld());
        }
        //Debug.Log($"Tile {hex.q},{hex.r} value {tile.Value}");
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
