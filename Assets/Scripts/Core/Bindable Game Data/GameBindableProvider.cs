using UnityEngine;

public class GameBindableProvider : MonoBehaviour
{
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
        return key switch
        {
            GameBindableKey.PlayerHealth => PlayerState.Instance.health,
            GameBindableKey.PlayerLight => PlayerState.Instance.light,
            _ => null,
        };
    }
}
