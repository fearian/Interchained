using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Validator
{
    private HexGrid _hexGrid;
    
    public Validator(HexGrid hexGrid)
    {
        _hexGrid = hexGrid;
    }

    public IEnumerable<TileData> NeighboursCanPair(TileData tile)
    {
        int tileValue = tile.Value;
        if (tileValue <= 0 || tileValue >= 7) yield break;

        foreach (Hex neighbour in tile.hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(neighbour)) continue;

            TileData otherTile = _hexGrid.GetTile(neighbour);
            int neighbourValue = otherTile.Value;
            if (neighbourValue <= 0 || neighbourValue >= 7) continue;

            int smaller = Mathf.Min(tileValue, neighbourValue);
            int larger = Mathf.Max(tileValue, neighbourValue);

            if (smaller % 2 == 1 && larger == smaller + 1)
            {
                yield return otherTile;
            }
        }
    }

    public IEnumerable<TileData> IsDuplicatedAlongAxis(TileData thisTile)
    {
        int i = 0;
        if (thisTile.IsEmpty || !thisTile.IsNumber) yield break;

        foreach (Hex cell in thisTile.hex.AlongAxis(_hexGrid.GridDiameter))
        {
            if (!_hexGrid.ValidHexes.Contains(cell)) continue;

            TileData foundTile = _hexGrid.GetTile(cell);
            if (foundTile.IsEmpty) continue;
            
            if (thisTile.Value == foundTile.Value && !thisTile.hex.Equals(cell))
            {
                DebugUtils.DrawDebugHex(cell.ToWorld(), 3f);
                i++;
                yield return foundTile;
            }
        }
        // if we detect a duplicate, we must be invalid too.
        if (i > 0) yield return thisTile;
    }

    public IEnumerable<TileData> IsDuplicatedInRegion(TileData thisTile)
    {
        int i = 0;
        if (thisTile.IsEmpty || thisTile.region == 0) yield break;

        int region = (int)thisTile.region - 1;
        foreach (Hex cell in _hexGrid.Regions[region])
        {
            TileData foundTile = _hexGrid.GetTile(cell);
            if (foundTile.IsEmpty) continue;
            if (thisTile.Value == foundTile.Value && !thisTile.hex.Equals(cell))
            {
                DebugUtils.DrawDebugHex(cell.ToWorld(), 3f);
                i++;
                yield return foundTile;
            }
        }
        // if we detect a duplicate, we must be invalid too.
        if (i > 0) yield return thisTile;
    }

    public IEnumerable<TileData> GearIsStuckOnGear(TileData thisTile)   
    {
        if (thisTile.IsEmpty || thisTile.region == 0) yield break;

        int i = 0;
        foreach (Hex neighbour in thisTile.hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(neighbour)) continue;

            TileData neighbouringTile = _hexGrid.GetTile(neighbour);
            if (neighbouringTile.IsEmpty) continue;
            
            if (thisTile.Value == neighbouringTile.Value && !thisTile.hex.Equals(neighbour))
            {
                i++;
                DebugUtils.DrawDebugHex(neighbour.ToWorld(), 3f);
                yield return neighbouringTile;
            }

        }
        if (i > 0) yield return thisTile;
    }

    public IEnumerable<TileData> IsTouchingLoopIncorrectly(TileData thisTile)
    {
        if (thisTile.region == 0) yield break;
        if (thisTile.IsMarkedForLoop == false) yield break;

        foreach (Hex neighbour in thisTile.hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(neighbour)) continue;
            
            TileData neighbouringTile = _hexGrid.GetTile(neighbour);
            if (neighbouringTile.IsMarkedForLoop == false) continue;
            
            if (SidesTouchingLoop(neighbour) >= 3)
            {
                yield return neighbouringTile;
            }
        }

        if (SidesTouchingLoop(thisTile.hex) >= 3) yield return thisTile;
    }

    public int SidesTouchingLoop(Hex hex)
    {
        TileData thisTile = _hexGrid.GetTile(hex);
        if (thisTile.region == 0) return 0;

        int i = 0;
        foreach (Hex neighbour in hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(neighbour)) continue;
            
            TileData neighbouringTile = _hexGrid.GetTile(neighbour);
            if (neighbouringTile.IsMarkedForLoop) i++;
        }

        return i;
    }

    public IEnumerable<TileData> FindAdjacent(TileData tile, TileData[] targets)
    {
        if (tile.region == 0) yield break;

        foreach (Hex neighbour in tile.hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(neighbour)) continue;

            TileData neighbouringTile = _hexGrid.GetTile(neighbour);
            if (targets.Contains(neighbouringTile)) yield return neighbouringTile;
        }
    }

    public bool InvalidNumber(TileData tile)
    {
        var axis = IsDuplicatedAlongAxis(tile);
        var region = IsDuplicatedInRegion(tile);

        return (axis.Count() != 0 || region.Count() != 0);
    }

    public bool InvalidGear(TileData tile)
    {
        var neighbours = GearIsStuckOnGear(tile);
        var region = IsDuplicatedInRegion(tile);

        return (neighbours.Count() != 0 || region.Count() != 0);
    }

    public bool InvalidLoop(TileData tile)
    {
        var touchingLoopTiles = IsTouchingLoopIncorrectly(tile);
        
        return (touchingLoopTiles.Count() != 0);
    }
}
