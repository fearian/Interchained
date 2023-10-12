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
            if (tile.IsInvalid || !tile.IsPaired || !tile.IsNumber) return false;
        }

        possibleLoopTiles = loopTiles;

        return true;
    }

    public void TryToDrawLoop(TileData[] loopTiles)
    {
        ClearLoop();
        if (EvaluateLoopTiles(loopTiles) == false) return;
        if (loopTiles.Length < 2) return;

        bool msg = false;
        if (msg) ClearMsg();
        
        
        TileData currentTile = loopTiles[0];
        TileData startingTile = loopTiles[0];
        
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
                Debug.DrawLine(currentTile.hex.ToWorld(), currentTile.LoopOut.hex.ToWorld() + new Vector3(0.1f,0,0.1f), Color.blue, 15f);
                Debug.DrawLine(currentTile.LoopIn.hex.ToWorld() + new Vector3(0.1f,0,-0.1f), currentTile.hex.ToWorld(), Color.red, 15f);
                
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
    }

    public void ClearLoop()
    {
        LineRenderer.positionCount = 0;
    }
}
