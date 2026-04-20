using System;
using UnityEngine;

public enum GameBindableKey
{
    PlayerHealth = 0,
    PlayerLight = 1,
}

public static class GameBindableRegistry
{
    private static Func<GameBindableKey, object> _resolver;

    public static void SetResolver(Func<GameBindableKey, object> resolver)
    {
        _resolver = resolver;
    }

    public static void ClearResolver()
    {
        _resolver = null;
    }

    public static T Resolve<T>(GameBindableKey key) where T : class
    {
        return _resolver?.Invoke(key) as T;
    }
}
