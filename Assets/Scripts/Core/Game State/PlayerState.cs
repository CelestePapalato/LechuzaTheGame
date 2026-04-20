using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; private set; } = null;

    public IntRangeBinding health = new();
    public new IntRangeBinding light = new();

    public static UnityAction<int, int> OnLightChange;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetHealth(int current, int max)
    {
        health?.SetValues(current, max);
    }

    public void SetLight(int current, int max)
    {
        light?.SetValues(current, max);
        OnLightChange?.Invoke(current, max);
    }
}
