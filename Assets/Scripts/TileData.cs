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
    public int Value { get; private set; } = 0;
    public bool IsEmpty => Value == 0;
    public bool IsNumber => Value is >= 1 and <= 7;
    public bool IsInvalid { get; private set; } = false;
    public bool IsOnLoop { get; private set; } = false;
    public bool IsLocked { get; private set; } = false;
    public bool IsHidden { get; private set; } = false;

    public UnityEvent onValueChanged;
    public UnityEvent onStatusChanged;

    public int SetValue(int newValue)
    {
        if (newValue < 0 || newValue > Constants.MAX_CELL_VALUE)
        {
            //bool wasNumber = _isNumber;
            Debug.Log($"VALUE: '{Value}' is invalid, Set 0 instead");
            newValue = 0;
            //if (_isNumber != wasNumber) onStatusChanged.Invoke();
            //if (_isNumber) isOnLoop = false;
        }
        Value = newValue;

        onValueChanged.Invoke();

        return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public void ToggleIsLoop()
    {
        if (!IsNumber) return;
        IsOnLoop = !IsOnLoop;

        onStatusChanged.Invoke();
    }
    public void ToggleIsLoop(bool isLoop)
    {
        if (!IsNumber) return;
        IsOnLoop = isLoop;

        onStatusChanged.Invoke();
    }

    public void ClearTile()
    {
        Value = 0;
        onValueChanged.Invoke();
        IsOnLoop = false;
        onStatusChanged.Invoke();
    }
}

