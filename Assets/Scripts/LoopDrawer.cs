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

    public LoopDrawer Initialize(HexGrid hexGrid, Validator validator)
    {
        _board = hexGrid;
        _validator = validator;

        return this;
    }

    private bool TryCacheLoopTiles(TileData[] loopTiles)
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

    public void BuildLoopGraph(TileData[] loopTiles)
    {
        possibleLoopTiles = null;
        if (TryCacheLoopTiles(loopTiles) == false) return;
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
        if (TryCacheLoopTiles(loopTiles) == false) return;
        if (loopTiles.Length < 2) return;

        ClearMsg();

        TileData currentTile = loopTiles[0];
        TileData startingTile = loopTiles[0];
        AddMsg(startingTile.hex, "Start!");

        int step = 0;
        while (step < loopTiles.Length)
        {
            TileData[] AdjacentLoops = FindAdjacentLoopTiles(currentTile);

            if (AdjacentLoops == null) break;

            AssignInOutFromAdjacents(currentTile, AdjacentLoops);

            void AssignInOutFromAdjacents(TileData tileData, TileData[] adjacentSteps)
            {
                if (tileData.LoopIn == null)
                {
                    if (adjacentSteps.Length >= 2)
                        tileData.SetLoopEndpoints(adjacentSteps[0], adjacentSteps[1]);
                    else
                        tileData.SetLoopEndpoints(adjacentSteps[0], null);
                }
                else
                {
                    foreach (TileData adjacentLoopTile in adjacentSteps)
                    {
                        if (tileData.LoopIn == adjacentLoopTile) continue;
                        else
                        {
                            tileData.SetLoopEndpoints(tileData.LoopIn, adjacentLoopTile);
                            adjacentLoopTile.SetLoopEndpoints(tileData, adjacentLoopTile.LoopOut);
                        }
                    }
                }
            }

            if (currentTile.LoopOut != null)
            {
                currentTile = currentTile.LoopOut;
                step++;
                AddMsg(currentTile.hex, $"step {step}", true);
            }
            else break;
        }

        if (step == loopTiles.Length - 1)
        {
            AddMsg(currentTile.hex, $"loop @ {step}", true);
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
        TileData[] AdjacentLoops = _validator.AdjacentOf(tile, possibleLoopTiles)
            .OrderBy(t => DirectionIndex(tile.hex, t.hex))
            .ToArray();

        if (AdjacentLoops.Count() < 1 || AdjacentLoops.Count() > 2)
        {
            return null;
        }

        if (AdjacentLoops.Any(t => t == null))
        {
            Debug.LogWarning("found 2 valid steps for the loop, but ended up as null in array?");
            return null;
        }

        return AdjacentLoops;
    }

    private int DirectionIndex(Hex from, Hex to)
    {
        Hex delta = to - from;
        for (int i = 0; i < Hex.AXIAL_DIRECTIONS.Length; i++)
        {
            if (Hex.AXIAL_DIRECTIONS[i].q == delta.q && Hex.AXIAL_DIRECTIONS[i].r == delta.r) return i;
        }
        return -1;
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
