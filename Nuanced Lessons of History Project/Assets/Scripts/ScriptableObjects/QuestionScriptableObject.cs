using UnityEngine;
using UnityEngine.Localization;

public class QuestionScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] protected LocalizedString _question;
    [SerializeField] protected LocalizedString[] _answers;
    #endregion

    #region Properties
    public LocalizedString Question => _question;
    public LocalizedString[] Answers => _answers;
    #endregion
}
