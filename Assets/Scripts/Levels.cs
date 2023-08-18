using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level List", menuName = "Interchained/Levels List", order = 1)]
public class Levels : ScriptableObject
{
    public List<BoardData1D<int>> savedLevels;

    public void AddLevel(BoardData1D<int> level)
    {
        savedLevels.Add(level);
    }

    public BoardData1D<int> GetLevel(string name)
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
