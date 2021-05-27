using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    #region Fields
    [Header("Game Manager")]
    [Range(60, 144)]
    [SerializeField] private int _maxFPS = 144;
    #endregion

    #region Properties
    #endregion

    #region Methods
    protected override void OnAwake()
    {
        base.OnAwake();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _maxFPS;
    }
    #endregion
}
