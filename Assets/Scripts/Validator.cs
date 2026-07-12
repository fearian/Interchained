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

    public IEnumerable<TileData> AdjacentOf(TileData tile, TileData[] targets)
    {
        if (tile.region == 0) yield break;
        if (targets == null) yield break;

        foreach (Hex neighbour in tile.hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(neighbour)) continue;

            TileData neighbouringTile = _hexGrid.GetTile(neighbour);
            
            if (neighbouringTile != null && targets.Contains(neighbouringTile)) yield return neighbouringTile;
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

        return (neighbours.Count() != 0 || region.Count() != 0 || InvalidGearLoopMeshing(tile));
    }

    public bool InvalidLoop(TileData tile)
    {
        var touchingLoopTiles = IsTouchingLoopIncorrectly(tile);
        
        return (touchingLoopTiles.Count() != 0);
    }

    public int GetDirectionBetween(Hex from, Hex to)
    {
        Hex delta = to - from;
        for (int i = 0; i < Hex.AXIAL_DIRECTIONS.Length; i++)
        {
            if (Hex.AXIAL_DIRECTIONS[i].q == delta.q && Hex.AXIAL_DIRECTIONS[i].r == delta.r) return i;
        }
        return -1;
    }

    public int GetLoopSide(TileData gear, TileData loopTile)
    {
        if (gear == null || loopTile == null) return 0;
        if (!loopTile.IsMarkedForLoop || loopTile.IsInvalid) return 0;
        if (loopTile.LoopIn == null) return 0;

        int entryDir = GetDirectionBetween(loopTile.LoopIn.hex, loopTile.hex);
        int gearDir = GetDirectionBetween(loopTile.hex, gear.hex);
        if (entryDir < 0 || gearDir < 0) return 0;

        int diff = (gearDir - entryDir + 6) % 6;
        if (diff == 1 || diff == 2) return 1;
        if (diff == 4 || diff == 5) return -1;
        return 0;
    }

    public bool InvalidGearLoopMeshing(TileData gear)
    {
        if (gear == null || !gear.IsGear) return false;

        int gearSide = GearLoopSide(gear);
        if (gearSide == 0) return false;

        foreach (Hex hex in _hexGrid.ValidHexes)
        {
            TileData other = _hexGrid.GetTile(hex);
            if (other == null || !other.IsGear || other == gear) continue;

            int otherSide = GearLoopSide(other);
            if (otherSide == 0) continue;

            if (otherSide == gearSide && other.Value != gear.Value) return true;
            if (otherSide != gearSide && other.Value == gear.Value) return true;
        }
        return false;
    }

    private int GearLoopSide(TileData gear)
    {
        foreach (Hex nb in gear.hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(nb)) continue;
            TileData loopTile = _hexGrid.GetTile(nb);
            if (loopTile == null || !loopTile.IsMarkedForLoop) continue;
            int side = GetLoopSide(gear, loopTile);
            if (side != 0) return side;
        }
        return 0;
    }
}
