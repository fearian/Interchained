using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level List", menuName = "Interchained/Levels List", order = 1)]
public class Levels : ScriptableObject
{
    public List<BoardData> savedLevels;
    public List<String> listOfLevels;

    public void AddLevel(BoardData level)
    {
        savedLevels.Add(level);
        listOfLevels.Add(level.Name);
    }

    public BoardData GetLevel(string name)
    {
        foreach (var level in savedLevels)
        {
            Debug.Log($"Compare {level.Name} against {name}");
            if (level.Name == name)
            {
                Debug.Log("MATCH!");
                return level;
            }
        }
        Debug.LogWarning($"Level \"{name}\" not found in saved levels.");
        return null;
    }
}
