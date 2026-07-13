using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphTester : MonoBehaviour
{
    public TileData[] Tiles;

    private LoopGraph graph = new LoopGraph();
    public List<LoopNode> nodes = new List<LoopNode>();

    private void Start()
    {
        RunHasCycleTests();
        LoadGraph(Tiles);
    }

    private void RunHasCycleTests()
    {
        if (Tiles == null || Tiles.Length < 6)
        {
            Debug.LogWarning("MVP-006: HasCycle tests need at least 6 Tiles assigned in the inspector");
            return;
        }

        LoopGraph openChain = BuildGraph(
            new[] { Tiles[0], Tiles[1], Tiles[2], Tiles[3] },
            new[,] { { 0, 1 }, { 1, 2 }, { 2, 3 } });
        Debug.Assert(!openChain.HasCycle(), "MVP-006: open chain should have no cycle");

        LoopGraph triangle = BuildGraph(
            new[] { Tiles[0], Tiles[1], Tiles[2] },
            new[,] { { 0, 1 }, { 1, 2 }, { 2, 0 } });
        Debug.Assert(triangle.HasCycle(), "MVP-006: triangle should have a cycle");

        LoopGraph twoCycles = BuildGraph(
            new[] { Tiles[0], Tiles[1], Tiles[2], Tiles[3], Tiles[4], Tiles[5] },
            new[,] { { 0, 1 }, { 1, 2 }, { 2, 0 }, { 3, 4 }, { 4, 5 }, { 5, 3 } });
        Debug.Assert(twoCycles.HasCycle(), "MVP-006: two disjoint cycles should report a cycle");

        LoopGraph tJunction = BuildGraph(
            new[] { Tiles[0], Tiles[1], Tiles[2], Tiles[3] },
            new[,] { { 0, 1 }, { 1, 2 }, { 1, 3 }, { 2, 3 } });
        Debug.Assert(tJunction.HasCycle(), "MVP-006: T-junction containing a cycle should report a cycle");

        Debug.Log("MVP-006 HasCycle tests passed");
    }

    private LoopGraph BuildGraph(TileData[] tiles, int[,] edges)
    {
        LoopGraph g = new LoopGraph();
        LoopNode[] nodes = new LoopNode[tiles.Length];
        for (int i = 0; i < tiles.Length; i++) nodes[i] = new LoopNode(tiles[i]);

        for (int e = 0; e < edges.GetLength(0); e++)
        {
            g.AddNode(nodes[edges[e, 0]], new[] { nodes[edges[e, 1]] });
        }
        return g;
    }

    private void RunIsValidTests()
    {
        var zero = new LoopNode(null);
        Debug.Assert(!zero.IsValid, "MVP-001: 0-link node should be invalid");

        var one = new LoopNode(null);
        one.addLink(new LoopNode(null));
        Debug.Assert(one.IsValid, "MVP-001: 1-link node should be valid");

        var two = new LoopNode(null);
        two.addLink(new LoopNode(null));
        two.addLink(new LoopNode(null));
        Debug.Assert(two.IsValid, "MVP-001: 2-link node should be valid");

        var three = new LoopNode(null);
        three.addLink(new LoopNode(null));
        three.addLink(new LoopNode(null));
        three.addLink(new LoopNode(null));
        Debug.Assert(!three.IsValid, "MVP-001: 3-link node should be invalid");

        Debug.Log("MVP-001 IsValid operator tests passed");
    }

    private void LoadGraph(TileData[] tiles)
    {

        for (int i = 0; i < tiles.Length; i++)
        {
            nodes.Add( new LoopNode(Tiles[i]) );
        }
        
        graph.AddNode(nodes[0], new LoopNode[2] { nodes[1], nodes[3] });
        graph.AddNode(nodes[1], new LoopNode[2] { nodes[2], nodes[4] });
        graph.AddNode(nodes[2], new LoopNode[1] { nodes[5] });
        graph.AddNode(nodes[3], new LoopNode[1] { nodes[4] });
        graph.AddNode(nodes[4], new LoopNode[2] { nodes[1], nodes[5] });
        graph.AddNode(nodes[5], new LoopNode[3] { nodes[4], nodes[6], nodes[2] });
        graph.AddNode(nodes[6], new LoopNode[1] { nodes[5] });

        return;
        
        foreach (var tile in Tiles)
        {
            var node = new LoopNode(tile);
            graph.AddNode(node);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var node in graph.Nodes)
        {
            var position = node.Tile.hex.ToWorld();
            DebugExtension.DebugPoint(position, Color.yellow, 1f);

            foreach (var link in node.Links)
            {
                var direction = link.To.Tile.hex.ToWorld() - position;
                DebugExtension.DrawArrow(position, direction, Color.blue);
            }
        }
    }
}
