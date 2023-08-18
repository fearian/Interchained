using System.Collections;
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
    
    public IEnumerable<Hex> IsDuplicatedAlongAxis(Hex hex)
    {
        TileData thisTile = _hexGrid.GetTile(hex);
        if (thisTile.IsEmpty || !thisTile.IsNumber) yield break;

        foreach (Hex cell in hex.AlongAxis(_hexGrid.GridDiameter))
        {
            if (!_hexGrid.ValidHexes.Contains(cell)) continue;

            TileData tile = _hexGrid.GetTile(cell);
            if (tile.IsEmpty) continue;
                DebugUtils.DrawDebugHex(cell.ToWorld(), 0.5f);
            
            if (thisTile.Value == tile.Value)
            {
                DebugUtils.DrawDebugHex(cell.ToWorld(), 2.5f);
                yield return cell;
            }
        }
    }
    
    
}
