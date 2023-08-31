using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tile Label Palette", menuName = "Interchained/Tile Labels", order = 2)]
public class TileLabels : ScriptableObject
{
    public string one = "1";
    public string two = "2";
    public string three = "A";
    public string four = "B";
    public string five = "X";
    public string six = "Y";
    public string seven = "7";

    public string GetLabel(int value)
    {
        switch (value)
        {
            case 1: return one;
            case 2: return two;
            case 3: return three;
            case 4: return four;
            case 5: return five;
            case 6: return six;
            case 7: return seven;
            default: return value.ToString();
        } 
    }
}
