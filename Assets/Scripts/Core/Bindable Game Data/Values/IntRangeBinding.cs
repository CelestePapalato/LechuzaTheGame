using System;

public class IntRangeBinding
{
    public int Current { get; private set; }
    public int Max { get; private set; }
    public event Action<int, int> ValuesChanged;

    public void SetValues(int current, int max)
    {
        Current = current;
        Max = max;
        ValuesChanged?.Invoke(current, max);
    }
}