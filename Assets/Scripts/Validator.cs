using System.Collections.Generic;
using System.Linq;

public class Validator
{
    private HexGrid _hexGrid;
    
    public Validator(HexGrid hexGrid)
    {
        _hexGrid = hexGrid;
    }
    
    public IEnumerable<Hex> IsDuplicatedAlongAxis(Hex hex)
    {
        int i = 0;
        TileData thisTile = _hexGrid.GetTile(hex);
        if (thisTile.IsEmpty || !thisTile.IsNumber) yield break;

        foreach (Hex cell in hex.AlongAxis(_hexGrid.GridDiameter))
        {
            if (!_hexGrid.ValidHexes.Contains(cell)) continue;

            TileData tile = _hexGrid.GetTile(cell);
            if (tile.IsEmpty) continue;
            
            if (thisTile.Value == tile.Value && !hex.Equals(cell))
            {
                DebugUtils.DrawDebugHex(cell.ToWorld(), 3f);
                i++;
                yield return cell;
            }
        }
        // if we detect a duplicate, we must be invalid too.
        if (i > 0) yield return hex;
    }

    public IEnumerable<Hex> IsDuplicatedInRegion(Hex hex)
    {
        int i = 0;
        TileData thisTile = _hexGrid.GetTile(hex);
        if (thisTile.IsEmpty || thisTile.region == 0) yield break;

        int region = (int)thisTile.region - 1;
        foreach (Hex cell in _hexGrid.Regions[region])
        {
            TileData tile = _hexGrid.GetTile(cell);
            if (tile.IsEmpty) continue;
            if (thisTile.Value == tile.Value && !hex.Equals(cell))
            {
                DebugUtils.DrawDebugHex(cell.ToWorld(), 3f);
                i++;
                yield return cell;
            }
        }
        // if we detect a duplicate, we must be invalid too.
        if (i > 0) yield return hex;
    }

    public IEnumerable<Hex> GearIsStuckOnGear(Hex hex)
    {
        TileData thisTile = _hexGrid.GetTile(hex);
        if (thisTile.IsEmpty || thisTile.region == 0) yield break;

        int i = 0;
        foreach (Hex neighbour in hex.Neighbours())
        {
            if (!_hexGrid.ValidHexes.Contains(neighbour)) continue;

            TileData neighbouringTile = _hexGrid.GetTile(neighbour);
            if (neighbouringTile.IsEmpty) continue;
            
            if (thisTile.Value == neighbouringTile.Value && !hex.Equals(neighbour))
            {
                i++;
                DebugUtils.DrawDebugHex(neighbour.ToWorld(), 3f);
                yield return neighbour;
            }

        }
        if (i > 0) yield return hex;
    }

    public bool InvalidNumber(Hex hex)
    {
        var axis = IsDuplicatedAlongAxis(hex);
        var region = IsDuplicatedInRegion(hex);

        return (axis.Count() != 0 || region.Count() != 0);
    }

    public bool InvalidGear(Hex hex)
    {
        var neighbours = GearIsStuckOnGear(hex);
        var region = IsDuplicatedInRegion(hex);

        return (neighbours.Count() != 0 || region.Count() != 0);
    }

}
