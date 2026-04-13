using System;

public enum GameState
{
    PLAYING,
    UI_SCENE,
    CUTSCENE,
    LOADING,
    DEATH,
    GAME_END
}

public static class GameStateEvents
{
    public static event Action<GameState> OnGameStateUpdate;

    public static void BroadcastStateChange(GameState newState)
    {
        OnGameStateUpdate?.Invoke(newState);
    }
}