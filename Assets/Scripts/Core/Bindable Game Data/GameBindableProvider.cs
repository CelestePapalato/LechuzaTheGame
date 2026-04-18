using UnityEngine;

public class GameBindableProvider : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;

    private void Awake()
    {
        GameBindableRegistry.SetResolver(Resolve);
    }

    private void OnDestroy()
    {
        GameBindableRegistry.ClearResolver();
    }

    private object Resolve(GameBindableKey key)
    {
        switch (key)
        {
            case GameBindableKey.PlayerHealth:
                return playerState.health;
            default:
                return null;
        }
    }
}
