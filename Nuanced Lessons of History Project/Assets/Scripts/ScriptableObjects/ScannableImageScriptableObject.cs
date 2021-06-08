using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "S_ScannableImage", menuName = "Scriptable Objects/ScannableImage")]
public class ScannableImageScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private ScannableImageType _imageType;
    [SerializeField] private LocalizedString _info;
    [SerializeField] private LocalizedString _hint;
    [HorizontalLine(1)]
    [SerializeField] private SpecialAction _onImageScanAction;
    #endregion

    #region Properties
    public ScannableImageType ImageType => _imageType;
    public LocalizedString Info => _info;
    public LocalizedString Hint => _hint;
    public SpecialAction OnImageScanAction => _onImageScanAction;
    #endregion
}
