using UnityEngine;
using UnityEngine.SceneManagement;
using NaughtyAttributes;

public class GameManager : Singleton<GameManager>
{
    #region Fields
    [Header("Game Manager")]
    [Range(60, 144)]
    [SerializeField] private int _maxFPS = 144;
    [HorizontalLine(1)]
    [Header("Restart")]
    [SerializeField] private GameObject _restartPopup;
    [SerializeField] private int _afkTimer = 30;

    private bool _gameHasStarted;
    private float _timeAFK;
    #endregion

    #region Properties
    public int MaxFPS => _maxFPS;
    #endregion

    #region Methods
    protected override void OnAwake()
    {
        base.OnAwake();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _maxFPS;
        _timeAFK = _afkTimer;
    }

    private void Update()
    {
        if (!_gameHasStarted) return;

        _timeAFK -= Time.deltaTime;
        if (Input.anyKey || Input.touchCount > 0) _timeAFK = _afkTimer;

        if (_timeAFK > 0) return;

        _restartPopup.SetActive(true);
        _timeAFK = _afkTimer;
    }

    public void StartGame()
    {
        SoundManager.Instance.StopAllAmbient();
        DialogueManager.Instance.StartNewGame();
        _gameHasStarted = true;
    }

    public void RestartGame()
    {
        _gameHasStarted = false;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void Quit()
    {
        Application.Quit();
    }
    #endregion
}
