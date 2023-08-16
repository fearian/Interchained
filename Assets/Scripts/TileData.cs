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
    public int value { get; private set; } = 0;
    private bool _isEmpty => value == 0;
    private bool _isNumber => value is >= 1 and <= 7;
    public bool isValid { get; private set; } = true;
    public bool isOnLoop { get; private set; } = false;

    public UnityEvent onValueChanged;
    public UnityEvent onStatusChanged;

    public int SetValue(int newValue)
    {
        if (newValue < 0 || newValue > Constants.MAX_CELL_VALUE)
        {
            //bool wasNumber = _isNumber;
            Debug.Log($"VALUE: '{value}' is invalid, Set 0 instead");
            newValue = 0;
            //if (_isNumber != wasNumber) onStatusChanged.Invoke();
            //if (_isNumber) isOnLoop = false;
        }
        value = newValue;

        onValueChanged.Invoke();

        return value;
    }

    public override string ToString()
    {
        return value.ToString();
    }

    public void ToggleIsLoop()
    {
        if (!_isNumber) return;
        isOnLoop = !isOnLoop;

        onStatusChanged.Invoke();
    }

    public void ClearTile()
    {
        value = 0;
        onValueChanged.Invoke();
        isOnLoop = false;
        onStatusChanged.Invoke();
    }
}

