using UnityEngine;

public class LanguageIcon : MonoBehaviour
{
    #region Fields
    [SerializeField] private Sprite _selected;
    [SerializeField] private Sprite _unselected;
    #endregion

    #region Properties
    public Sprite Selected { get => _selected; set => _selected = value; }
    public Sprite Unselected { get => _unselected; set => _unselected = value; }
    #endregion
}
