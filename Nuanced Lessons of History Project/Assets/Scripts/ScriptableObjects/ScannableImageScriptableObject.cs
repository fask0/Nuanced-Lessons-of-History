using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "S_ScannableImage", menuName = "Scriptable Objects/ScannableImage")]
public class ScannableImageScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private ScannableImageType _imageType;
    [SerializeField] private LocalizedString _info;
    [SerializeField] private Sprite _hintSprite;
    [HorizontalLine(1)]
    [SerializeField] private SpecialAction _onImageScanAction;
    [HorizontalLine(1)]
    [SerializeField] private bool _allowSkip;
    #endregion

    #region Properties
    public ScannableImageType ImageType => _imageType;
    public LocalizedString Info => _info;
    public Sprite HintSprite => _hintSprite;
    public SpecialAction OnImageScanAction => _onImageScanAction;
    public bool AllowSkip => _allowSkip;
    #endregion
}
