using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService : IService
{
    public event Action<string> OnSceneLoaded;
    public event Action<string> OnSceneUnloaded;

    private MonoBehaviour _runner;

    public SceneService() { }

    public Task InitializeAsync()
    {
        var go = new GameObject("[SceneService]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<SceneServiceRunner>();

        SceneManager.sceneLoaded += HandleSceneLoaded;
        SceneManager.sceneUnloaded += HandleSceneUnloaded;

        return Task.CompletedTask;
    }

    public void LoadScene(string name, LoadSceneMode mode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(name, mode);
    }

    public void LoadSceneAsync(
        string name,
        Action<float> progressCallback = null,
        Action completionCallback = null,
        bool allowSceneActivation = true,
        LoadSceneMode mode = LoadSceneMode.Single)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name, mode);
        if (operation == null)
        {
            Debug.LogError($"[SceneService] Failed to start loading scene '{name}'.");
            return;
        }

        operation.allowSceneActivation = allowSceneActivation;
        _runner.StartCoroutine(TrackLoad(operation, progressCallback, completionCallback));
    }

    public Task UnloadSceneAsync(string name)
    {
        AsyncOperation operation = SceneManager.UnloadSceneAsync(name);
        if (operation == null)
        {
            Debug.LogError($"[SceneService] Failed to start unloading scene '{name}'.");
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>();
        _runner.StartCoroutine(TrackUnload(operation, tcs));
        return tcs.Task;
    }

    private static IEnumerator TrackLoad(
        AsyncOperation operation,
        Action<float> progressCallback,
        Action completionCallback)
    {
        while (!operation.isDone)
        {
            progressCallback?.Invoke(operation.progress);
            yield return null;
        }

        progressCallback?.Invoke(1f);
        completionCallback?.Invoke();
    }

    private static IEnumerator TrackUnload(AsyncOperation operation, TaskCompletionSource<bool> tcs)
    {
        while (!operation.isDone)
            yield return null;

        tcs.SetResult(true);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnSceneLoaded?.Invoke(scene.name);
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        OnSceneUnloaded?.Invoke(scene.name);
    }

    private sealed class SceneServiceRunner : MonoBehaviour { }
}
