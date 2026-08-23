using System.Collections;
using Lechuza.UI;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] Health _playerHealth;
    [SerializeField] ScreenNodeSO gameOverScreen;
    [SerializeField] ScreenNodeSO levelCompletedScreen;

    [Header("Settings")]
    [SerializeField] float waitTimeBeforeGameOver = 2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnEnable()
    {
        _playerHealth.OnDeath += GameOver;
    }

    private void OnDisable()
    {
        _playerHealth.OnDeath -= GameOver;
    }

    private void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(waitTimeBeforeGameOver);
        Services.Get<ScreenService>().GoTo(gameOverScreen);
    }

    public void LevelCompleted()
    {
        Services.Get<ScreenService>().GoTo(levelCompletedScreen);
    }
}
