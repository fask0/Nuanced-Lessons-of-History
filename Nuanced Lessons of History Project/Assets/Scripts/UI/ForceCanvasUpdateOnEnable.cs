using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceCanvasUpdateOnEnable : MonoBehaviour
{
    #region Fields
    [SerializeField] private GameObject[] _gosToRefresh;
    #endregion

    #region Methods
    private void OnEnable()
    {
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < _gosToRefresh.Length; i++)
        {
            _gosToRefresh[i].SetActive(false);
            _gosToRefresh[i].SetActive(true);
        }
    }
    #endregion
}
