using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interchained : MonoBehaviour
{
    [SerializeField][Range(3,9)]
    private int size = 7;
    [SerializeField]
    private TileData tileObject;
    [SerializeField] public BoardColors boardColorPalette;

    private HexGrid hexGrid;

    private Vector3 mousePos;
    void Start()
    {
        hexGrid = new HexGrid(size, tileObject);
    }

    private void Update()
    {
        mousePos = GetMouseWorldPosition();
        //DebugUtils.DrawDebugHex(new Vector3(mousePos.x, 0, mousePos.z), 0.3f);
        if (Input.GetMouseButtonDown(0)) CycleValues(true);
        if (Input.GetMouseButtonDown(1)) CycleValues(false);
    }

    private void CycleValues(bool cycleUp = true)
    {
        Hex hex = Hex.FromWorld(mousePos); 
        DebugUtils.DrawDebugHex(hex.ToWorld(), 0.1f);
        TileData tile = hexGrid.GetTile(hex);
        if (tile == null) return;
        if (cycleUp) tile.SetValue(tile.value + 1);
        else tile.SetValue(tile.value - 1);
        Debug.Log($"Tile {hex.q},{hex.r} value {tile.value}");
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return worldPosition;
    }
}
