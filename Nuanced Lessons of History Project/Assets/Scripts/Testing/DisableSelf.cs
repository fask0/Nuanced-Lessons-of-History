using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableSelf : MonoBehaviour
{
    [SerializeField] private float _disableDelay;

    private void OnEnable()
    {
        Invoke(nameof(disableSelf), _disableDelay);
    }

    private void disableSelf()
    {
        gameObject.SetActive(false);
    }
}
