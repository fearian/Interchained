using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public static class Constants
{
    public const int MAX_CELL_VALUE = 9;
}



public class TileData : Node
{
    public BoardRegions region;  
    public int Value { get; private set; } = 0;
    public bool IsEmpty => (Value == 0);
    public bool IsNumber => Value is >= 1 and <= 7;
    public bool IsGear => Value is 8 or 9;
    public bool IsBlocker => Value is 10;
    public bool IsInvalid { get; private set; } = false;
    public bool IsLocked { get; private set; } = false;
    public bool IsHidden { get; private set; } = false;
    
    //Loop
    public bool IsMarkedForLoop { get; private set; } = false;
    public TileData LoopIn = null;
    public TileData LoopOut = null;
    //public bool IsOnLoopIncorrectly { get; private set; } = false;
    
    //Pairs
    public TileData pairedTile { get; private set; }
    public bool IsPaired => pairedTile != null;
    public bool IsLowerOfPair => (IsPaired && Value % 2 == 1);


    public UnityEvent onValueChanged;
    public UnityEvent onLoopChanged;
    
    [SerializeField] public ParticleSystem loopParticles;

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

    public void SetPairedTile(TileData newPair)
    {
        pairedTile = newPair;

        pairedTile.onValueChanged.AddListener(OnPairedTileValueChanged);
        
        onValueChanged.Invoke();
    }

    private bool IsPairValid()
    {
        if (pairedTile == null || pairedTile.IsPaired == false) return false;

        bool thisIsOdd = (Value % 2 == 1);
        if (thisIsOdd && pairedTile.Value == Value + 1) return true;
        else if (!thisIsOdd && pairedTile.Value == Value - 1) return true;
        else return false;
    }

    private void OnPairedTileValueChanged()
    {
        if (IsPairValid() == false)
        {
            pairedTile.onValueChanged.RemoveListener(OnPairedTileValueChanged);
            pairedTile = null;
            onValueChanged.Invoke();
        }
    }

    public void RemovePair()
    {
        pairedTile.onValueChanged.RemoveListener(OnPairedTileValueChanged);
        pairedTile = null;
        onValueChanged.Invoke();
    }

    public void MarkForLoop(bool isLoop)
    {
        IsMarkedForLoop = isLoop;
        
        if (!IsMarkedForLoop)
        {
            LoopIn = null;
            LoopOut = null;
        }
        
        onValueChanged.Invoke();
        onLoopChanged.Invoke();
    }
    
    public void ToggleIsLoop()
    {
        MarkForLoop(!IsMarkedForLoop);
    }

    public void MarkAsInvalid(bool isInvalid = true)
    {
        IsInvalid = isInvalid;
        onValueChanged.Invoke();
        if (IsMarkedForLoop) onLoopChanged.Invoke();
    }
    public void MarkAsValid(bool isValid = true)
    {
        IsInvalid = !isValid;
        onValueChanged.Invoke();
        if (IsMarkedForLoop) onLoopChanged.Invoke();
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
            IsMarkedForLoop = false;
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
        IsMarkedForLoop = false;
        LoopIn = null;
        LoopOut = null;
        IsInvalid = false;
        onValueChanged.Invoke();
        onLoopChanged.Invoke();
    }
}

