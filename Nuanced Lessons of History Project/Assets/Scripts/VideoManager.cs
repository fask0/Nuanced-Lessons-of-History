using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.UI;

public class VideoManager : Singleton<VideoManager>
{
    #region Fields
    [HorizontalLine(1)]
    [Header("Panels")]
    [SerializeField] private GameObject _videoPanel;
    [SerializeField] private GameObject _vnPanel;
    [SerializeField] private GameObject _arPanel;

    private VideoPlayer _player;
    private VideoClip _currentClip;
    private Coroutine _playing = null;
    #endregion

    #region Methods
    private void Start()
    {
        _videoPanel.SetActive(false);
        _player = GetComponent<VideoPlayer>();
    }

    public void PlayNewClip(VideoClip pClip, float pStartDelay = 0)
    {
        if (_playing != null) StopCoroutine(_playing);

        _playing = StartCoroutine(playClip(pClip, pStartDelay));
    }

    public void Stop()
    {
        if (_playing != null)
            _player.Stop();
    }

    public void Pause()
    {
        if (!_player.isPaused)
            _player.Pause();
    }

    public void Resume()
    {
        if (_player.isPaused)
            _player.Play();
    }

    private IEnumerator playClip(VideoClip pClip, float pStartDelay = 0)
    {
        yield return new WaitForSeconds(pStartDelay);

        _vnPanel.SetActive(false);
        _arPanel.SetActive(false);
        _currentClip = pClip;
        _player.clip = _currentClip;
        _player.Play();
        for (int i = 0; i < 20; i++)
            yield return new WaitForEndOfFrame();
        _videoPanel.SetActive(true);

        UnityAction onClickAction = () => onClick();
        _videoPanel.GetComponent<Button>().onClick.AddListener(onClickAction);
        bool shouldBreak = false;
        float timeRemaining = (float)_currentClip.length;
        while (_player.isPlaying || _player.isPaused)
        {
            yield return new WaitForEndOfFrame();
            timeRemaining -= Time.deltaTime;
            if (shouldBreak || timeRemaining <= 0) break;
        }
        _videoPanel.GetComponent<Button>().onClick.RemoveListener(onClickAction);

        _videoPanel.SetActive(false);
        _playing = null;
        DialogueManager.Instance.Resume();

        #region Local Methods
        void onClick()
        {
            shouldBreak = true;
        }
        #endregion
    }
    #endregion
}
