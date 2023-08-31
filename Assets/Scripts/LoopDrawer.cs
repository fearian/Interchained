using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoopDrawer : MonoBehaviour
{
    private Validator _validator;
    private HexGrid _board = null;

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

        return true;
    }

    public void TryToDrawLoop(TileData[] loopTiles, TileData startingTile = null)
    {
        ClearLoop();
        if (EvaluateLoopTiles(loopTiles) == false) return;
        if (loopTiles.Length < 4) return;
        
        
        
        //LineRenderer.positionCount = loopTiles.Length;
        
        TileData currentTile = startingTile;
        if (startingTile == null)
        {
            currentTile = loopTiles[0];
        }

        int step = 0;
        Debug.Log("Begin Loop Stepping");
        while (step < loopTiles.Length)
        {
            var validSteps = _validator.FindAdjacentLoops(currentTile);
            if (validSteps.Count() != 2)
            {
                Debug.Log("found !2 adjacent Loops :((");
                break;
            }

            Debug.Log("found 2 adjacent Loops");

            foreach (TileData validStep in validSteps)
            {
                if (!loopTiles.Contains(validStep)) continue;
                if (currentTile.LoopIn == null) currentTile.LoopIn = validStep;
                if (currentTile.LoopIn != validStep)
                {
                    currentTile.LoopOut = validStep;
                    validStep.LoopIn = currentTile;
                }
            }

            if (currentTile.LoopOut != null)
            {
                Debug.DrawLine(currentTile.hex.ToWorld(), currentTile.LoopOut.hex.ToWorld(), Color.blue, 1f);
                if (currentTile.LoopIn != null) Debug.DrawLine(currentTile.LoopIn.hex.ToWorld(), currentTile.hex.ToWorld(), Color.blue, 1f);
                
                currentTile = currentTile.LoopOut;
                step++;
                
            }
            else break;
        }

        if (step == loopTiles.Length)
        {
            //LineRenderer.loop = true;
            for (int i = 0; i < loopTiles.Length; i++)
            {
                //LineRenderer.SetPosition(i, currentTile.hex.ToWorld(0.125f));
                currentTile = currentTile.LoopOut;
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
        LineRenderer.positionCount = 0;
    }
}
