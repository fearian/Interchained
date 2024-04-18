using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoopGraph
{
    public List<LoopNode> Nodes { get; private set; }
    private Dictionary<Hex, LoopNode> hexToNodeMap = new Dictionary<Hex, LoopNode>();
    public int Count { get { return Nodes.Count; } }

    public LoopGraph()
    {
        Nodes = new List<LoopNode>();
    }

    public void AddNode(LoopNode node, LoopNode[] linkedNodes = null)
    {
        if (!hexToNodeMap.ContainsKey(node.Tile.hex))
        {
            Nodes.Add(node);
            hexToNodeMap[node.Tile.hex] = node;
        }

        if (linkedNodes != null)
        {
            foreach (var linkedNode in linkedNodes)
            {
                if (linkedNode != null)
                {
                    if (!Nodes.Contains(linkedNode))
                    {
                        AddNode(linkedNode);
                    }
                    AddLink(node, linkedNode);
                }
            }
        }
    }

    public void RemoveNode(LoopNode node)
    {
        //TODO: Refactor this
        // When removing a node, find all links that point to that node, and remove them
        
        Link[] links = node.Links.ToArray();

        foreach (Link link in links)
        {
            if (link.IsDirectional == false)
            {
                link.To.removeLink(node);
            }
        }

        hexToNodeMap.Remove(node.Tile.hex);
        Nodes.Remove(node);
        
    }
    
    private void AddLink(LoopNode from, LoopNode to, bool isDirectional = false)
    {
        if (!hexToNodeMap.ContainsKey(from.Tile.hex)) {
            Nodes.Add(from);
            hexToNodeMap[from.Tile.hex] = from;
        }
        if (!hexToNodeMap.ContainsKey(to.Tile.hex)) {
            Nodes.Add(to);
            hexToNodeMap[to.Tile.hex] = to;
        }
        
        var link = new Link(from, to, isDirectional);
        from.Links.Add(link);

        if (!isDirectional) 
        {
            // Update links of the adjacent node for undirected edges
            if (to.findLink(from) == null)
            {
                to.Links.Add(new Link(to, from, isDirectional));
            }
        }
    }

    public void FlipDirection(LoopNode from, LoopNode to)
    {
        if (hexToNodeMap.ContainsKey(from.Tile.hex) && hexToNodeMap.ContainsKey(to.Tile.hex)) {
            from.removeLink(to);
            to.addLink(from, true);
        }
        else
        {
            Debug.LogWarning($"Tried to Flip Direction of {from.findLink(to).ToString()}, but nodes missing");
        }
    }
    
    public bool IsValid()
    {
        return Nodes.All(node => node.IsValid); 
    }
    
    private LoopNode FindNode(TileData tileData) 
    {
        if (hexToNodeMap.TryGetValue(tileData.hex, out var node))
        {
            return node;
        }
        return null; 
    }

    private bool ContainsNode(LoopNode loopNode)
    {
        if (Nodes.Contains(loopNode)) return true;
        else return false;
    }

    public List<LoopNode> FindPathDFS(LoopNode start, LoopNode end)
    {
        // ... Implementation of Depth-First Search
        throw new NotImplementedException();
    }

    public List<LoopNode> FindPathBFS(LoopNode start, LoopNode end)
    {
        // ... Implementation of Breadth-First Search
        throw new NotImplementedException();
    }
    
    public bool HasCycle()
    {
        // ... Implementation of a cycle detection algorithm
        throw new NotImplementedException();
    }
}