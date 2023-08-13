using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Constants
{
    public const int MAX_CELL_VALUE = 9;
}

public class TileData : Node
{
    [SerializeField][Range(0, Constants.MAX_CELL_VALUE)]
    public int value = 0;//{ get; private set; } = 0;
    private bool _isEmpty => value == 0;
    private bool _isNumber => value is >= 1 and <= 7; 
    private bool _isValid;
    private bool _isOnLoop;

    public UnityEvent onValueChanged;

    public int SetValue(int newValue)
    {
        if (newValue < 0 || newValue > Constants.MAX_CELL_VALUE)
        {
            Debug.Log($"VALUE: '{value}' is invalid, Set 0 instead");
            newValue = 0;
        }
        value = newValue;

        onValueChanged.Invoke();

        return value;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

