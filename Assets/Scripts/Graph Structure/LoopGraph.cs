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

    public bool HasCycle()
    {
        if (Nodes.Count == 0) return false;

        HashSet<LoopNode> visited = new HashSet<LoopNode>();
        foreach (LoopNode start in Nodes)
        {
            if (visited.Contains(start)) continue;
            if (CycleDFS(start, null, visited)) return true;
        }
        return false;
    }

    private bool CycleDFS(LoopNode node, LoopNode parent, HashSet<LoopNode> visited)
    {
        visited.Add(node);

        foreach (LoopNode neighbour in DistinctNeighbors(node))
        {
            if (!visited.Contains(neighbour))
            {
                if (CycleDFS(neighbour, node, visited)) return true;
            }
            else if (neighbour != parent)
            {
                return true;
            }
        }
        return false;
    }

    public List<LoopNode> FindPathDFS(LoopNode start, LoopNode end)
    {
        if (start == null || end == null) return null;

        HashSet<LoopNode> visited = new HashSet<LoopNode>();
        List<LoopNode> path = new List<LoopNode>();
        if (DFSVisit(start, end, visited, path)) return path;
        return null;
    }

    private bool DFSVisit(LoopNode current, LoopNode end, HashSet<LoopNode> visited, List<LoopNode> path)
    {
        visited.Add(current);
        path.Add(current);

        if (current == end) return true;

        foreach (LoopNode neighbour in DistinctNeighbors(current))
        {
            if (visited.Contains(neighbour)) continue;
            if (DFSVisit(neighbour, end, visited, path)) return true;
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    public List<LoopNode> FindPathBFS(LoopNode start, LoopNode end)
    {
        if (start == null || end == null) return null;

        HashSet<LoopNode> visited = new HashSet<LoopNode>();
        Dictionary<LoopNode, LoopNode> parent = new Dictionary<LoopNode, LoopNode>();
        Queue<LoopNode> queue = new Queue<LoopNode>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            LoopNode current = queue.Dequeue();
            if (current == end) break;

            foreach (LoopNode neighbour in DistinctNeighbors(current))
            {
                if (visited.Contains(neighbour)) continue;
                visited.Add(neighbour);
                parent[neighbour] = current;
                queue.Enqueue(neighbour);
            }
        }

        if (!visited.Contains(end)) return null;

        List<LoopNode> path = new List<LoopNode>();
        LoopNode node = end;
        while (node != start)
        {
            path.Add(node);
            node = parent[node];
        }
        path.Add(start);
        path.Reverse();
        return path;
    }

    private IEnumerable<LoopNode> DistinctNeighbors(LoopNode node)
    {
        HashSet<LoopNode> seen = new HashSet<LoopNode>();
        foreach (Link link in node.Links)
        {
            if (link.To == null) continue;
            if (seen.Add(link.To)) yield return link.To;
        }
    }
}