using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpriteLooper : MonoBehaviour
{
    #region Fields
    [SerializeField] private Sprite[] _backgroundFrames;
    [SerializeField] [Range(1, 144)] private int _fps;

    private Coroutine _updating = null;
    #endregion

    #region Methods
    private void OnEnable()
    {
        if (_backgroundFrames.Length == 0) return;
        if (_updating != null) StopCoroutine(_updating);
        _updating = StartCoroutine(update());
    }

    private void OnDisable()
    {
        StopCoroutine(_updating);
        _updating = null;
    }

    private IEnumerator update()
    {
        int index = 0;
        Image image = GetComponent<Image>();
        while (gameObject.activeSelf)
        {
            image.sprite = _backgroundFrames[index];

            if (index + 1 >= _backgroundFrames.Length) index = 0;
            else index++;

            yield return new WaitForSeconds(1.0f / _fps);
        }
    }
    #endregion
}
