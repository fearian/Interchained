using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board Color Palette", menuName = "Interchained/Board Colors", order = 1)]
public class BoardColors : ScriptableObject
{
    [Header("Tile Value Colors")]
    public Color pair12 = Color.green;
    public Color pair34 = Color.blue;
    public Color pair56 = Color.red;
    public Color tile7 = Color.yellow;
    public Color gears = Color.white;
    
    [Header("Board Colors")]
    public Color regionNorth = Color.gray;
    public Color regionEast = Color.gray;
    public Color regionSouth = Color.gray;
    public Color regionWest = Color.gray;
    [Space] public Color invalidTile = Color.red;
}
