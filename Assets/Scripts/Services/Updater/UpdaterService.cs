using System.Threading.Tasks;
using UnityEngine;

public class UpdaterService : IService
{
    private Updater _updater;

    public UpdaterService() { }

    public Task InitializeAsync()
    {
        var go = new GameObject("[Updater]");
        Object.DontDestroyOnLoad(go);
        _updater = go.AddComponent<Updater>();
        return Task.CompletedTask;
    }

    public void Register(object target)
    {
        if (_updater == null) return;
        _updater.Register(target);
    }

    public void Unregister(object target)
    {
        if (_updater == null) return;
        _updater.Unregister(target);
    }
}
