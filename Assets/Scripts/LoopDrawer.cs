using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static HexMsg;

public class LoopDrawer : MonoBehaviour
{
    private Validator _validator;
    private HexGrid _board = null;
    private TileData[] possibleLoopTiles;
    private LoopGraph sortingGraph = new LoopGraph();

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

    public void SortLoopTiles(TileData[] loopTiles)
    {
        if (EvaluateLoopTiles(loopTiles) == false) return;
        sortingGraph = new LoopGraph();
        Debug.Log("Starting new sort graph sort! ooh!");
        
        //add tiles as nodes, with adjacent as links.
        foreach (var tile in loopTiles)
        {
            DebugExtension.DebugPoint(tile.hex.ToWorld() + new Vector3(0,1,0), Color.yellow, 0.1f);
            TileData[] AdjacentLoops = FindAdjacentLoopTiles(tile);
            if (AdjacentLoops == null) break;
            LoopNode[] adjacentNodes = AdjacentLoops.Select(tile => new LoopNode(tile)).ToArray();
            
            sortingGraph.AddNode(new LoopNode(tile), adjacentNodes);
        }
    }
    
    public void TryToDrawLoop(TileData[] loopTiles)
    {
        ClearLoop();
        if (EvaluateLoopTiles(loopTiles) == false) return;
        if (loopTiles.Length < 2) return;

        bool msg = true;
        if (msg) ClearMsg();
        
        // Pick a starting tile
        TileData currentTile = loopTiles[0];
        TileData startingTile = loopTiles[0];
        if (msg) AddMsg(startingTile.hex, "Start!");

        // Step through every tile and assign in/out links
        int step = 0;
        while (step < loopTiles.Length)
        {
            
            // Find and evaluate Adjacent Loop Tiles
            TileData[] AdjacentLoops = FindAdjacentLoopTiles(currentTile);

            if (AdjacentLoops == null) break;

            AssignInOutFromAdjacents(currentTile, AdjacentLoops);

            void AssignInOutFromAdjacents(TileData tileData, TileData[] adjacentSteps)
            {
                //If the tile has not previously been linked to by another tile
                if (tileData.LoopIn == null)
                {
                    tileData.LoopIn = adjacentSteps[0];
                    tileData.LoopOut = adjacentSteps[1];
                }
                //If the tile has an "in" linked from a previous tile
                else
                {
                    foreach (TileData adjacentLoopTile in adjacentSteps)
                    {
                        //for each step, it's either the assigned in, or we assign it to out.
                        if (tileData.LoopIn == adjacentLoopTile) continue;
                        else
                        {
                            tileData.LoopOut = adjacentLoopTile;
                            adjacentLoopTile.LoopIn = tileData;
                        }
                    }
                }
            }

            if (currentTile.LoopOut != null)
            {
                //Debug Lines
                //Vector3 inDir = Vector3.Normalize((currentTile.hex.ToWorld()) - currentTile.LoopIn.hex.ToWorld()) * 0.4f;
                //Vector3 outDir = Vector3.Normalize((currentTile.LoopOut.hex.ToWorld()) - currentTile.hex.ToWorld()) * 0.4f;
                //DebugExtension.DebugArrow((currentTile.hex.ToWorld() - inDir) + new Vector3(0,0.6f,0.01f), inDir, new Color(0.1f, 0.85f, 0.51f), 25f);
                //DebugExtension.DebugArrow(currentTile.hex.ToWorld() + new Vector3(0,0.6f,-0.01f), outDir, new Color(0.85f, 0.1f, 0.51f), 25f);
                //DebugExtension.DebugPoint(currentTile.hex.ToWorld() + new Vector3(0,0.6f,0), new Color(0.85f, 0.85f, 0.5f), 0.15f, 25f);
                
                currentTile = currentTile.LoopOut;
                step++;
                if (msg) AddMsg(currentTile.hex, $"step {step}", true);
            }
            else break;
        }

        if (step == loopTiles.Length - 1)
        {
            if (msg) AddMsg(currentTile.hex, $"loop @ {step}", true);
            if (currentTile.LoopOut == startingTile) DrawLoop(true);
            else DrawLoop(false);
        }
        else return;

        void DrawLoop(bool isClosed = false)
        {
            Debug.Log($"Step@{step}, LoopTiles@{loopTiles.Length}, Drawing loop!");
            int drawLength;
            if (isClosed) drawLength = loopTiles.Length + 1;
            else drawLength = loopTiles.Length;
            LineRenderer.positionCount = drawLength;
            LineRenderer.loop = true;
            for (int i = 0; i < drawLength; i++)
            {
                LineRenderer.SetPosition(i, currentTile.hex.ToWorld(0.125f));
                if (currentTile.LoopIn == null)
                {
                    LineRenderer.positionCount = i + 1;
                    LineRenderer.loop = false;
                    return;
                }
                else currentTile = currentTile.LoopIn;
            }
        }
    }

    private TileData[] FindAdjacentLoopTiles(TileData tile)
    {
        TileData[] AdjacentLoops = _validator.FindInAdjacent(tile, possibleLoopTiles).ToArray();
        
        if (AdjacentLoops.Count() < 1 || AdjacentLoops.Count() > 2)
        {
            return null;
        }

        if (AdjacentLoops.Any(tile => tile == null))
        {
            Debug.LogWarning("found 2 valid steps for the loop, but ended up as null in array?");
            return null;
        }

        return AdjacentLoops;
    }

    public void ClearLoop()
    {
        LineRenderer.positionCount = 0;
    }
    
    private void OnDrawGizmos()
    {
        if (sortingGraph == null) return;
        foreach (var node in sortingGraph.Nodes)
        {
            var offset = new Vector3(0,1,0);
            var position = node.Tile.hex.ToWorld();
            DebugExtension.DebugPoint(position + offset, new Color(0.85f, 0.85f, 0.5f), 0.3f);
            //new Color(0.15f, 0.85f, 0.51f)
            foreach (var link in node.Links)
            {
                var direction = link.To.Tile.hex.ToWorld() - position;
                DebugExtension.DrawArrow(position + offset, direction * 0.5f , new Color(0.85f, 0.1f, 0.51f));
            }
        }
    }
    
}
