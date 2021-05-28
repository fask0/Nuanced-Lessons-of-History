using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableSidebarElement : MonoBehaviour, IExpandableUIElement
{
    public enum TransitionState
    {
        Expanding,
        Collapsing,
        Expanded,
        Collapsed
    }

    #region Fields
    [SerializeField] private RectTransform _dummyRect;
    private RectTransform _rect;
    private Coroutine _transforming;
    private ARManager _arManager;
    private TransitionState _state;
    #endregion

    #region Properties
    public Button DummyButton => _dummyRect.GetComponent<Button>();
    public TransitionState State => _state;
    #endregion

    #region Methods
    public void Init()
    {
        _rect = GetComponent<RectTransform>();
        _transforming = null;
        _arManager = ARManager.Instance;
        _state = TransitionState.Collapsed;
    }

    public void Expand(Vector2 pTargetSize, float pTransitionDuration) { }

    public void Expand(float pTransitionDuration)
    {
        if (_transforming != null) { _arManager.StopCoroutine(_transforming); _transforming = null; }
        _transforming = _arManager.StartCoroutine(expand(pTransitionDuration));
    }

    private IEnumerator expand(float pTransitionDuration)
    {
        gameObject.SetActive(false);

        _state = TransitionState.Expanding;
        Vector2 targetSize = _rect.sizeDelta;
        float timeElapsed = 0;
        while (Vector2.Distance(_dummyRect.sizeDelta, targetSize) > 0.01f)
        {
            float closeTime = pTransitionDuration;
            _dummyRect.sizeDelta = Vector2.Lerp(_dummyRect.sizeDelta, targetSize, timeElapsed / closeTime);
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _dummyRect.sizeDelta = targetSize;
        _dummyRect.gameObject.SetActive(false);
        gameObject.SetActive(true);
        _transforming = null;
        _state = TransitionState.Expanded;
    }

    public void Collapse(Vector2 pTargetSize, float pTransitionDuration)
    {
        if (_transforming != null) { _arManager.StopCoroutine(_transforming); _transforming = null; }
        _transforming = _arManager.StartCoroutine(collapse(pTargetSize, pTransitionDuration));
    }

    public void Collapse(float pTransitionDuration) { }

    private IEnumerator collapse(Vector2 pTargetSize, float pTransitionDuration)
    {
        _dummyRect.gameObject.SetActive(true);
        gameObject.SetActive(false);

        _state = TransitionState.Collapsing;
        float timeElapsed = 0;
        while (Vector2.Distance(pTargetSize, _dummyRect.sizeDelta) > 0.01f)
        {
            float closeTime = pTransitionDuration;
            _dummyRect.sizeDelta = Vector2.Lerp(_dummyRect.sizeDelta, pTargetSize, timeElapsed / closeTime);
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _dummyRect.sizeDelta = pTargetSize;
        _transforming = null;
        _state = TransitionState.Collapsed;
    }
    #endregion
}
