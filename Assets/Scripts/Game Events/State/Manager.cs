using UnityEngine;

public abstract class Manager : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        GameStateEvents.OnGameStateUpdate += OnStateUpdate;
    }

    protected virtual void OnDisable()
    {
        GameStateEvents.OnGameStateUpdate -= OnStateUpdate;
    }

    private void OnStateUpdate(GameState newState)
    {
        switch (newState)
        {
            case GameState.PLAYING:
                HandlePlayingState();
                break;
            case GameState.UI_SCENE:
                HandleUISceneState();
                break;
            case GameState.CUTSCENE:
                HandleCutsceneState();
                break;
            case GameState.LOADING:
                HandleLoadingState();
                break;
            case GameState.DEATH:
                HandleDeadState();
                break;
            case GameState.GAME_END:
                HandleGameEndState();
                break;
        }
    }

    protected virtual void HandlePlayingState() { }
    protected virtual void HandleUISceneState() { }
    protected virtual void HandleCutsceneState() { }
    protected virtual void HandleLoadingState() { }
    protected virtual void HandleDeadState() { }
    protected virtual void HandleGameEndState() { }
}
