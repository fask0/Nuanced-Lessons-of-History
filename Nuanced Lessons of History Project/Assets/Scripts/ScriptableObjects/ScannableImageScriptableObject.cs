using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "ScannableImage", menuName = "Scriptable Objects/ScannableImage")]
public class ScannableImageScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private ScannableImageType _imageType;
    [SerializeField] private LocalizedString _info;
    [SerializeField] private LocalizedString _hint;
    #endregion

    #region Properties
    public ScannableImageType ImageType => _imageType;
    public LocalizedString Info => _info;
    public LocalizedString Hint => _hint;
    #endregion
}
