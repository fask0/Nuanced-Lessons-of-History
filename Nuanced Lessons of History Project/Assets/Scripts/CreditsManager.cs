using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEngine.UI;

public class CreditsManager : Singleton<CreditsManager>
{
    #region Fields
    [HorizontalLine(1)]
    [Header("Credits")]
    [SerializeField] private float _initialDelay;
    [SerializeField] private float _displayDuration;
    [SerializeField] private float _fadeInTime;
    [SerializeField] private float _fadeOutTime;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _skipButton;

    private CanvasGroup _canvasGroup;
    private List<GameObject> _canvasGroupChildren = new List<GameObject>();
    private Coroutine _fading = null;
    private int _progress;
    #endregion

    #region Methods
    private void OnEnable()
    {
        _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        for (int i = 0; i < _canvasGroup.transform.childCount; i++)
        {
            _canvasGroupChildren.Add(_canvasGroup.transform.GetChild(i).gameObject);
            _canvasGroup.transform.GetChild(i).gameObject.SetActive(false);
        }
        _progress = 0;

        if (_fading != null) StopCoroutine(_fading);
        _fading = StartCoroutine(fade(_initialDelay));
    }

    private void OnDisable()
    {
        StopCoroutine(_fading);
    }

    private IEnumerator fade(float pInitialDelay = 0)
    {
        _skipButton.onClick.RemoveAllListeners();

        UnityAction skipToNextChildAction = () => skipToNextChild();
        _skipButton.onClick.AddListener(skipToNextChildAction);

        //Fadein
        _canvasGroup.alpha = 0;
        _canvasGroup.transform.GetChild(_progress).gameObject.SetActive(true);
        yield return new WaitForSeconds(pInitialDelay);
        float fadeInTimeElapsed = 0;
        while (_canvasGroup.alpha < 1)
        {
            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 1, fadeInTimeElapsed / _fadeOutTime);
            yield return new WaitForEndOfFrame();
            fadeInTimeElapsed += Time.deltaTime;
        }
        _canvasGroup.alpha = 1;

        //Display
        float timeRemaining = _displayDuration;
        while (timeRemaining > 0)
        {
            yield return new WaitForEndOfFrame();
            timeRemaining -= Time.deltaTime;
        }
        _skipButton.onClick.RemoveListener(skipToNextChildAction);

        //Fadeout
        float fadeOutTimeElapsed = 0;
        while (_canvasGroup.alpha > 0)
        {
            _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 0, fadeOutTimeElapsed / _fadeOutTime);
            yield return new WaitForEndOfFrame();
            fadeOutTimeElapsed += Time.deltaTime;
        }
        _canvasGroup.alpha = 0;

        _canvasGroup.transform.GetChild(_progress).gameObject.SetActive(false);
        _progress++;
        if (_progress >= _canvasGroup.transform.childCount)
            _backButton.onClick.Invoke();
        else
            _fading = StartCoroutine(fade());

        #region Local Methods
        void skipToNextChild()
        {
            timeRemaining = 0;
        }
        #endregion
    }
    #endregion
}
