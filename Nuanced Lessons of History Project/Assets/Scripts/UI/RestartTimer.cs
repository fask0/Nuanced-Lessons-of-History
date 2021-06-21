using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RestartTimer : MonoBehaviour
{
    #region Fields
    [SerializeField] private float _timeBeforeRestart = 10;
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private string _timerFormat;

    private Coroutine _restarting = null;
    #endregion

    #region Methods
    private void OnEnable()
    {
        if (_restarting != null) StopCoroutine(_restarting);
        _restarting = StartCoroutine(timer());
    }

    private void OnDisable()
    {
        StopCoroutine(_restarting);
    }

    private IEnumerator timer()
    {
        float timeRemaining = _timeBeforeRestart;
        while (timeRemaining > 0)
        {
            yield return new WaitForEndOfFrame();
            timeRemaining -= Time.deltaTime;
            _timerText.text = string.Format(_timerFormat, Mathf.Ceil(timeRemaining).ToString());
        }

        GameManager.Instance.RestartGame();
    }
    #endregion
}
