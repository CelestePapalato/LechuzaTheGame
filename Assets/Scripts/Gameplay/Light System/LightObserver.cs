using UnityEngine;
using UnityEngine.Events;

public class LightObserver : MonoBehaviour
{
    public UnityEvent<int> OnLightChange;

    private void OnEnable()
    {
        PlayerState.OnLightChange += HandleLightChange;
    }

    private void OnDisable()
    {
        PlayerState.OnLightChange -= HandleLightChange;
    }

    private void HandleLightChange(int current, int max)
    {
        OnLightChange?.Invoke(current);
    }
}
