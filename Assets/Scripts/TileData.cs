using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public static class Constants
{
    public const int MAX_CELL_VALUE = 9;
}



public class TileData : Node
{
    public int Value { get; private set; } = 0;
    public bool IsEmpty => (Value == 0);
    public bool IsNumber => Value is >= 1 and <= 7;
    public bool IsGear => Value is 8 or 9;
    public bool IsInvalid { get; private set; } = false;
    public bool IsOnLoop { get; private set; } = false;
    public bool IsOnLoopIncorrectly { get; private set; } = false;
    public bool IsLocked { get; private set; } = false;
    public bool IsHidden { get; private set; } = false;
    public BoardRegions region;  

    public UnityEvent onValueChanged;
    [FormerlySerializedAs("onStatusChanged")] public UnityEvent onLoopChanged;

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
        IsOnLoop = !IsOnLoop;

        onValueChanged.Invoke();
        onLoopChanged.Invoke();
    }
    public void ToggleIsLoop(bool isLoop)
    {
        IsOnLoop = isLoop;
        
        onValueChanged.Invoke();
        onLoopChanged.Invoke();
    }

    public void MarkInvalid(bool isInvalid, bool regardingLoop = false)
    {
        if (regardingLoop)
        {
            IsOnLoopIncorrectly = isInvalid;
            onLoopChanged.Invoke();
        }
        else
        {
            IsInvalid = isInvalid;
            onValueChanged.Invoke();
        }
    }

    public void ClearTile()
    {
        Value = 0;
        onValueChanged.Invoke();
        IsOnLoop = false;
        IsInvalid = false;
        onLoopChanged.Invoke();
    }
}

