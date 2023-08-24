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
    public bool IsBlocker => Value is 10;
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
        if (IsLocked) return Value;
        if (newValue < 0 || newValue > Constants.MAX_CELL_VALUE)
        {
            newValue = 0;
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

    public void MarkLocked(bool isLocked)
    {
        if (IsEmpty)
        {
            IsLocked = false;
            return;
        }
        IsLocked = isLocked;
        onValueChanged.Invoke();
    }

    public void MarkBlocker(bool isBlocker)
    {
        if (isBlocker)
        {
            Value = 10;
            IsLocked = true;
            IsOnLoop = false;
            IsInvalid = false;
            onValueChanged.Invoke();
            onLoopChanged.Invoke();
        }
        else
        {
            ClearTile(true);
        }
    }

    public void ClearTile(bool clearBlockers = false)
    {
        if (!IsBlocker || !IsLocked || clearBlockers)
        {
            IsLocked = false;
            Value = 0;
        }
        IsOnLoop = false;
        IsInvalid = false;
        onValueChanged.Invoke();
        onLoopChanged.Invoke();
    }
}

