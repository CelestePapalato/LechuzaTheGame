using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private readonly Queue<AudioSource> _pool = new();
    private readonly List<AudioSource> _active = new();

    private readonly AudioSource _musicSource;

    private readonly int _poolSize = 15;
    private readonly bool _canGrow = true;

    private void Awake()
    {
        for(int i = 0; i < _poolSize; i++)
        {
            AudioSource source = CreateSource($"SFX {i}");
            source.playOnAwake = false;
            _pool.Enqueue(source);
        }
    }

    private void Update()
    {
        // Devolver al pool mediante Update?
        // de esta forma devolvemos al pool los audios modificados por pitch y los que fueron detenidos
        // pero tal vez sea costoso
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            AudioSource source = _active[i];
            if (source.isPlaying)
                continue;

            _active.RemoveAt(i);
            ReturnSource(source);
        }
    }

    private AudioSource CreateSource(string label)
    {
        var go = new GameObject(label);
        go.transform.SetParent(transform);

        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    private AudioSource GetSource()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();

        return _canGrow ? CreateSource("SFX") : null;
    }

    private void ReturnSource(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        _pool.Enqueue(source);
    }

    public void PlaySfx(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null)
            return;

        AudioSource source = GetSource();
        if (source == null)
            return;

        source.transform.position = position;
        source.clip = clip;
        source.volume = volume;
        source.gameObject.SetActive(true);
        source.Play();

        _active.Add(source);
    }

    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (clip == null)
            return;

        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }
}
