using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class Music
{
    #region Fields
    [SerializeField] private AudioClip _clip;
    #endregion

    #region Properties
    public AudioClip Clip => _clip;
    #endregion
}

[System.Serializable]
public class SFX
{
    enum DelayTypeBefore
    {
        None,
        Fixed
    }

    enum DelayTypeAfter
    {
        None,
        Fixed,
        WaitUntillFinished,
        WaitUntillFinishedPlusFixed,
    }

    #region Fields
    [SerializeField] private DelayTypeBefore _delayBefore;
    [AllowNesting] [ShowIf("_delayBefore", DelayTypeBefore.Fixed)] [SerializeField] private float _secondsBefore;
    [HorizontalLine(1)]
    [SerializeField] private AudioClip _clip;
    [SerializeField] private bool _playDuringText = false;
    [HorizontalLine(1)]
    [AllowNesting] [ShowIf("_playDuringText", true)] [SerializeField] private DelayTypeAfter _delayAfter;
    [AllowNesting] [ShowIf("shouldShowFixedAfter")] [SerializeField] private float _secondsAfter;
    #endregion

    #region Properties
    public AudioClip Clip => _clip;
    public bool PlayDuringText => _playDuringText;

    public float DelayBefore
    {
        get
        {
            return _delayBefore switch
            {
                DelayTypeBefore.None => 0,
                DelayTypeBefore.Fixed => _secondsBefore,
                _ => 0,
            };
        }
    }

    public float DelayAfter
    {
        get
        {
            if (_playDuringText) return 0;

            return _delayAfter switch
            {
                DelayTypeAfter.None => 0,
                DelayTypeAfter.Fixed => _secondsAfter,
                DelayTypeAfter.WaitUntillFinished => _clip.length,
                DelayTypeAfter.WaitUntillFinishedPlusFixed => _clip.length + _secondsAfter,
                _ => 0,
            };
        }
    }

    private bool shouldShowFixedAfter => !_playDuringText && ((_delayAfter == DelayTypeAfter.Fixed) || (_delayAfter == DelayTypeAfter.WaitUntillFinishedPlusFixed));
    #endregion
}

public class SoundManager : Singleton<SoundManager>
{
    #region Fields
    [HorizontalLine(1)]
    [Header("Sound")]
    [SerializeField] private Music[] _musicClips;

    private AudioSource _musicSource;
    private AudioClip _currentMusicClip;

    private AudioSource _ambientSource;

    private readonly List<AudioSource> _soundSources = new List<AudioSource>();
    private AudioSource _soundSource;
    #endregion

    #region Methods
    private void Start()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        _musicSource = sources[0];
        _ambientSource = sources[1];
        _soundSource = sources[2];
        _soundSources.Add(_soundSource);
    }

    public void PlayNewAmbientClip(AudioClip pClip)
    {
        _ambientSource.clip = pClip;
        if (pClip.name == "NighttimeAmbient")
            _ambientSource.volume = 0.03f;
        else if (pClip.name == "OutsideAmbient")
            _ambientSource.volume = 0.05f;
        else
            _ambientSource.volume = 0.15f;
        _ambientSource.Play();
    }

    public void PlayNewSoundClip(AudioClip pClip)
    {
        bool shuoldMakeNewSource = true;
        for (int i = 0; i < _soundSources.Count; i++)
        {
            if (_soundSources[i].clip != null) continue;

            StartCoroutine(playSound(_soundSources[i], pClip));
            shuoldMakeNewSource = false;
            break;
        }

        if (shuoldMakeNewSource)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = _soundSource.outputAudioMixerGroup;
            newSource.playOnAwake = _soundSource.playOnAwake;
            newSource.loop = _soundSource.loop;
            newSource.priority = _soundSource.priority;
            newSource.volume = _soundSource.volume;
            newSource.pitch = _soundSource.pitch;
            newSource.spatialBlend = _soundSource.spatialBlend;
            _soundSources.Add(newSource);
            StartCoroutine(playSound(_soundSources[_soundSources.Count - 1], pClip));
        }
    }

    public void StopAllAmbient()
    {
        _ambientSource.Stop();
    }

    public void StopAllSounds()
    {
        for (int i = 0; i < _soundSources.Count; i++)
        {
            _soundSources[i].Stop();
            _soundSources[i].clip = null;
        }
    }

    private IEnumerator playSound(AudioSource pSource, AudioClip pClip)
    {
        pSource.clip = pClip;
        pSource.Play();

        yield return new WaitForSeconds(pSource.clip.length);

        pSource.Stop();
        pSource.clip = null;
    }
    #endregion
}
