using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static HexMsg;

public class LoopDrawer : MonoBehaviour
{
    private Validator _validator;
    private HexGrid _board = null;
    private TileData[] possibleLoopTiles;

    [SerializeField] private LineRenderer LineRenderer;

    public LoopDrawer Initialize(HexGrid hexGrid)
    {
        _board = hexGrid;
        _validator = new Validator(_board);

        return this;
    }

    private bool EvaluateLoopTiles(TileData[] loopTiles)
    {
        if (loopTiles == null) return false;
        
        foreach (var tile in loopTiles)
        {
            if (tile == null) return false;
            if (tile.IsInvalid || !tile.IsPaired || !tile.IsNumber || tile.IsOnLoopIncorrectly) return false;
        }

        possibleLoopTiles = loopTiles;

        return true;
    }

    public void TryToDrawLoop(TileData[] loopTiles, TileData startingTile = null)
    {
        ClearLoop();
        if (EvaluateLoopTiles(loopTiles) == false) return;
        if (loopTiles.Length < 4) return;

        bool msg = true;
        if (msg) ClearMsg();
        
        
        TileData currentTile = startingTile;
        if (startingTile == null || startingTile.IsOnLoopIncorrectly)
        {
            currentTile = loopTiles[0];
            startingTile = currentTile;
        }
        
        if (msg) AddMsg(startingTile.hex, "Start:");

        int step = 0;
        while (step < loopTiles.Length + 1)
        {
            TileData[] steps = new TileData[2];
            var validSteps = _validator.FindAdjacent(currentTile, possibleLoopTiles);
            if (validSteps.Count() != 2)
            {
                if (msg) AddMsg(currentTile.hex, "!2 steps", true);
                break;
            }
            else
            {
                steps = validSteps.ToArray();
                if (msg) AddMsg(currentTile.hex, "good", true);
            }

            if (steps[0] == null || steps[1] == null)
            {
                Debug.LogWarning("found 2 valid steps for the loop, but ended up as null in array?");
                break;
            }

            if (currentTile.LoopIn == null)
            {
                currentTile.LoopIn = steps[0];
                AddMsg(currentTile.LoopIn.hex, "in", true);
                currentTile.LoopOut = steps[1];
                AddMsg(currentTile.LoopOut.hex, "out", true);
            }
            else
            {
                foreach (TileData tile in steps)
                {
                    if (currentTile.LoopIn == tile) continue;
                    else
                    {
                        currentTile.LoopOut = tile;
                        AddMsg(currentTile.LoopOut.hex, "out", true);
                        tile.LoopIn = currentTile;
                        AddMsg(tile.LoopIn.hex, "in", true);
                    }
                }
            }

            if (currentTile.LoopOut != null)
            {
                Debug.DrawLine(currentTile.hex.ToWorld() + new Vector3(0,0,0.25f), currentTile.LoopOut.hex.ToWorld(), Color.blue, 6f);
                Debug.DrawLine(currentTile.LoopIn.hex.ToWorld(), currentTile.hex.ToWorld(), Color.red, 6f);
                
                currentTile = currentTile.LoopOut;
                step++;
                if (msg) AddMsg(currentTile.hex, $"step {step}", true);
            }
            else break;
        }

        if (step == loopTiles.Length + 1)
        {
            if (msg) AddMsg(currentTile.hex, $"loop @ {step}", true);
            if (currentTile.LoopOut == startingTile) DrawLoop(true);
            else DrawLoop(false);
        }
        else return;

        void DrawLoop(bool isClosed = false)
        {
            Debug.Log($"Step@{step}, LoopTiles@{loopTiles.Length}, Drawing loop!");
            LineRenderer.positionCount = loopTiles.Length;
            LineRenderer.loop = isClosed;
            for (int i = 0; i < loopTiles.Length; i++)
            {
                LineRenderer.SetPosition(i, currentTile.hex.ToWorld(0.125f));
                currentTile = currentTile.LoopIn;
            }
        }
        
        /*TileData prev = loopTiles[0];
        for (int i = 0; i < loopTiles.Length; i++)
        {
            if (loopTiles.Length == 0) return;
            
            LineRenderer.SetPosition(i, loopTiles[i].hex.ToWorld(0.125f));
            prev = loopTiles[i];
        }*/
    }

    public void ClearLoop()
    {
        Debug.Log("Clearing Loop");
        LineRenderer.positionCount = 0;
    }
}
