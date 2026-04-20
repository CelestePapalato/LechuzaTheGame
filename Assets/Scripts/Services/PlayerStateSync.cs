using UnityEngine;

public class PlayerHealthSync : MonoBehaviour
{
    [SerializeField] private PlayerState state;
    [SerializeField] private Health health;
    [SerializeField] private new Light light;

    private void OnEnable()
    {
        health.OnHealthChanged += SyncHealth;
        light.OnLightChanged += SyncLight;
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= SyncHealth;
        light.OnLightChanged -= SyncLight;
    }

    private void SyncHealth(int current, int max)
    {
        state.SetHealth(current, max);
    }

    private void SyncLight(int current, int max)
    {
        state.SetLight(current, max);
    }
}