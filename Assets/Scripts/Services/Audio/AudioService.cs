using System.Threading.Tasks;
using UnityEngine;

public class AudioService : IService
{
    private AudioPlayer _player;

    public AudioService() { }

    public Task InitializeAsync()
    {
        var go = new GameObject("[AudioService]");
        Object.DontDestroyOnLoad(go);

        _player = go.AddComponent<AudioPlayer>();
        return Task.CompletedTask;
    }

    public void PlaySfx(AudioClip clip, Vector3 position)
    {
        if (_player == null) return;
        _player.PlaySfx(clip, position);
    }

    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (_player == null) return;
        _player.PlayMusic(clip, loop);
    }

    public void StopMusic()
    {
        if (_player == null) return;
        _player.StopMusic();
    }
}
