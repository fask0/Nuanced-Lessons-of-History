using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class Sound
{
    #region Fields
    [SerializeField] private AudioClip _audioClip;
    #endregion

    #region Properties
    public AudioClip Clip => _audioClip;
    #endregion
}

public class SoundManager : Singleton<SoundManager>
{
    #region Fields
    [HorizontalLine(1)]
    [Header("Sound")]
    [SerializeField] private Sound[] _musicClips;

    private AudioSource _musicSource;
    private AudioClip _currentMusicClip;

    private AudioSource _soundSource;
    private AudioClip _currentSoundClip;
    #endregion

    #region Methods
    private void Start()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        _musicSource = sources[0];
        _soundSource = sources[1];
    }

    public void PlayNewSoundClip(AudioClip pClip)
    {
        _currentSoundClip = pClip;
        _soundSource.clip = _currentSoundClip;
        _soundSource.Play();
    }

    public void StopSound()
    {
        _soundSource.Stop();
    }
    #endregion
}
