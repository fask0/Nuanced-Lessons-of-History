using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARHintFoldout : MonoBehaviour
{
    enum State
    {
        FoldingOut,
        FoldedOut,
        FoldingIn,
        FoldedIn
    }

    #region Fields
    [SerializeField] private float _targetYScale;
    [SerializeField] private float _foldTime;

    private State _foldState;
    private RectTransform _rect;
    private Coroutine _folding = null;
    #endregion

    #region Methods
    private void Start()
    {
        _rect = GetComponent<RectTransform>();
        _rect.localScale = new Vector3(_rect.localScale.x, 0, _rect.localScale.z);
        _foldState = State.FoldedIn;
    }

    private void OnEnable()
    {
        _rect = GetComponent<RectTransform>();
        _rect.localScale = new Vector3(_rect.localScale.x, 0, _rect.localScale.z);
        _foldState = State.FoldedIn;
    }

    public void Toggle()
    {
        if (_foldState == State.FoldedOut || _foldState == State.FoldingOut)
            StartFoldin();
        else if (_foldState == State.FoldedIn || _foldState == State.FoldingIn)
            StartFoldout();
    }

    public void StartFoldout()
    {
        if (_folding != null) StopCoroutine(_folding);
        _folding = StartCoroutine(foldout());
    }

    private IEnumerator foldout()
    {
        _foldState = State.FoldingOut;
        float timeElapsed = 0;
        float startingYScale = _rect.localScale.y;
        float realFoldTime = _foldTime * (1 - (startingYScale / _targetYScale));
        while (_rect.localScale.y < _targetYScale)
        {
            timeElapsed += Time.deltaTime;
            _rect.localScale = new Vector3(_rect.localScale.x, Mathf.Lerp(startingYScale, _targetYScale, timeElapsed / realFoldTime), _rect.localScale.z);
            yield return new WaitForEndOfFrame();
        }
        _rect.localScale = new Vector3(_rect.localScale.x, _targetYScale, _rect.localScale.z);
        _foldState = State.FoldedOut;
        _folding = null;
    }

    public void StartFoldin()
    {
        if (_folding != null) StopCoroutine(_folding);
        _folding = StartCoroutine(Foldin());
    }

    public IEnumerator Foldin()
    {
        _foldState = State.FoldingIn;
        float timeElapsed = 0;
        float startingYScale = _rect.localScale.y;
        float realFoldTime = _foldTime * (startingYScale / _targetYScale);
        while (_rect.localScale.y > 0)
        {
            timeElapsed += Time.deltaTime;
            _rect.localScale = new Vector3(_rect.localScale.x, Mathf.Lerp(startingYScale, 0, timeElapsed / realFoldTime), _rect.localScale.z);
            yield return new WaitForEndOfFrame();
        }
        _rect.localScale = new Vector3(_rect.localScale.x, 0, _rect.localScale.z);
        _foldState = State.FoldedIn;
        _folding = null;
    }
    #endregion
}
