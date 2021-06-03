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
    [SerializeField] private GameObject _symbol;
    private RectTransform _rect;
    private Vector2 _collapsedSize;
    private Coroutine _transforming;
    private ARManager _arManager;
    private TransitionState _state;
    private Image _image;
    #endregion

    #region Properties
    public Button DummyButton => _dummyRect.GetComponent<Button>();
    public TransitionState State => _state;
    #endregion

    #region Methods
    private void Update()
    {
        _image.enabled = false;
        _image.enabled = true;
    }

    public void Init()
    {
        _rect = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _collapsedSize = _dummyRect.sizeDelta;
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
        Vector2 startSize = _dummyRect.sizeDelta;
        float timeElapsed = Time.deltaTime;
        float closeTime = pTransitionDuration * (Vector2.Distance(startSize, targetSize) / Vector2.Distance(_collapsedSize, targetSize));
        while (Vector2.Distance(_dummyRect.sizeDelta, targetSize) > 0.01f)
        {
            _dummyRect.sizeDelta = Vector2.Lerp(startSize, targetSize, timeElapsed / closeTime);
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
        Vector2 startSize = _dummyRect.sizeDelta;
        float timeElapsed = 0;
        float closeTime = pTransitionDuration * (Vector2.Distance(pTargetSize, startSize) / Vector2.Distance(pTargetSize, _rect.sizeDelta));
        while (Vector2.Distance(pTargetSize, _dummyRect.sizeDelta) > 0.01f)
        {
            _dummyRect.sizeDelta = Vector2.Lerp(startSize, pTargetSize, timeElapsed / closeTime);
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _dummyRect.sizeDelta = pTargetSize;
        _transforming = null;
        _state = TransitionState.Collapsed;
    }
    #endregion
}
