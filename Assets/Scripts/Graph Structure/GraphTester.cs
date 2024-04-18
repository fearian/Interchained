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
        LoadGraph(Tiles);
        
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
