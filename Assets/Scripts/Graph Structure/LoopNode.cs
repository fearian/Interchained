using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LoopNode
{
    public TileData Tile { get; private set; }
    public List<Link> Links { get; private set; }
    public bool IsValid
    {
        get { return (Links.Count <= 2 || Links.Count >= 1); }
    }

    public LoopNode(TileData tile)
    {
        Tile = tile;
        Links = new List<Link>();
    }

    public Link findLink(LoopNode endNode)
    {
        return Links.FirstOrDefault(link => link.From == endNode);
    }

    public void addLink(LoopNode endNode, bool isDirectional = false)
    {
        Links.Add(new Link(this, endNode, isDirectional));
    }

    public void removeLink(LoopNode endNode)
    {
        Link matchingLink = Links.FirstOrDefault(link => link.From == endNode);
        if (matchingLink != null) Links.Remove(matchingLink);
    }

    public override bool Equals(System.Object obj) {
        if (obj == null || GetType() != obj.GetType()) 
            return false;
        LoopNode loopNode = (LoopNode)obj;
        return (Tile == loopNode.Tile);
    }
    
    public override int GetHashCode() {
        return 31 * Tile.hex.q + 37 * Tile.hex.r + Tile.hex.GetHashCode();
    }
    
    public override string ToString() {
        return $"{Tile.hex},[{Links.Count()}]";
    }
}

//[System.Serializable]
public class Link
{
    public LoopNode From { get; set; }
    public LoopNode To { get; set; }
    public bool IsDirectional { get; set; }

    public Link(LoopNode from, LoopNode to, bool isDirectional = false)
    {
        From = from;
        To = to;
        IsDirectional = isDirectional;
    }
    
    public override string ToString() {
        return $"{From.Tile.hex}{(IsDirectional == true ? "-->" : "---")}{To.Tile.hex}]";
    }
}