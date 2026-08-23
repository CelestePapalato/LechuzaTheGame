using System.Threading.Tasks;
using Lechuza.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenService : IService
{
    private ScreenManager _manager;

    public string CurrentNodeId => _manager != null ? _manager.CurrentNodeId : null;

    public ScreenService() { }

    public Task InitializeAsync()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;
        TryBindManager();
        return Task.CompletedTask;
    }

    public bool GoTo(string nodeId)
    {
        if (!EnsureManager()) return false;
        return _manager.GoTo(nodeId);
    }

    public bool GoTo(ScreenNodeSO screen)
    {
        if (!EnsureManager()) return false;
        return _manager.GoTo(screen);
    }

    public void GoBack()
    {
        if (!EnsureManager()) return;
        _manager.GoBack();
    }

    public void PopAll()
    {
        if (!EnsureManager()) return;
        _manager.PopAll();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindManager();
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        if (_manager == null) return;
        if (_manager.gameObject.scene == scene)
            _manager = null;
    }

    private void TryBindManager()
    {
        if (_manager != null) return;
        _manager = Object.FindFirstObjectByType<ScreenManager>();
    }

    private bool EnsureManager()
    {
        if (_manager == null)
            TryBindManager();

        if (_manager != null) return true;

        Debug.LogWarning("[ScreenService] No ScreenManager bound in the current scene.");
        return false;
    }
}
