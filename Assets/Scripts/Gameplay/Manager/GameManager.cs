using UnityEngine;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Health _playerHealth;

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
        Debug.Log("Game Over");
    }

}
