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
        if (sortingGraph == null || sortingGraph.Count < 2) return;

        ClearMsg();

        LoopNode startNode = sortingGraph.Nodes[0];
        foreach (var node in sortingGraph.Nodes)
        {
            if (NeighborsOf(node).Count == 1) { startNode = node; break; }
        }

        List<TileData> ordered = new List<TileData>();
        HashSet<Hex> visited = new HashSet<Hex>();
        LoopNode prev = null;
        LoopNode current = startNode;
        bool closed = false;

        int guard = 0;
        while (current != null && guard <= sortingGraph.Count + 1)
        {
            guard++;
            if (visited.Contains(current.Tile.hex))
            {
                if (current == startNode && prev != null)
                {
                    prev.Tile.SetLoopEndpoints(prev.Tile.LoopIn, startNode.Tile);
                    startNode.Tile.SetLoopEndpoints(prev.Tile, startNode.Tile.LoopOut);
                    closed = true;
                }
                break;
            }

            visited.Add(current.Tile.hex);
            ordered.Add(current.Tile);

            List<LoopNode> neighbours = NeighborsOf(current);
            LoopNode next = null;
            foreach (var nb in neighbours)
            {
                if (prev != null && nb.Tile.hex.Equals(prev.Tile.hex)) continue;
                next = nb;
                break;
            }

            TileData inTile = (prev != null) ? prev.Tile : null;
            TileData outTile = (next != null) ? next.Tile : null;
            current.Tile.SetLoopEndpoints(inTile, outTile);

            if (next == null) break;

            prev = current;
            current = next;
        }

        DrawOrderedLoop(ordered, closed);
    }

    private List<LoopNode> NeighborsOf(LoopNode node)
    {
        List<LoopNode> result = new List<LoopNode>();
        HashSet<Hex> seen = new HashSet<Hex>();
        foreach (var link in node.Links)
        {
            if (link.To == null) continue;
            if (seen.Add(link.To.Tile.hex)) result.Add(link.To);
        }
        return result;
    }

    private void DrawOrderedLoop(List<TileData> ordered, bool closed)
    {
        if (ordered.Count == 0) return;

        int count = closed ? ordered.Count + 1 : ordered.Count;
        LineRenderer.positionCount = count;
        for (int i = 0; i < ordered.Count; i++)
        {
            LineRenderer.SetPosition(i, ordered[i].hex.ToWorld(0.125f));
        }
        if (closed)
        {
            LineRenderer.SetPosition(ordered.Count, ordered[0].hex.ToWorld(0.125f));
            LineRenderer.loop = true;
        }
        else
        {
            LineRenderer.loop = false;
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
