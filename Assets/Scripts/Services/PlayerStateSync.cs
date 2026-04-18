using UnityEngine;

public class PlayerHealthSync : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private PlayerState state;

    private void OnEnable()
    {
        health.OnHealthChanged += Sync;
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= Sync;
    }

    private void Sync(int current, int max)
    {
        state.SetHealth(current, max);
    }
}