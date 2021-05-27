using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Question", menuName = "ScriptableObjects/Question")]
public class QuestionScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private LocalizedString _question;
    [SerializeField] private LocalizedString[] _wrongAnswers;
    [SerializeField] private LocalizedString _correctAnswer;
    #endregion

    #region Properties
    public LocalizedString Question => _question;
    public LocalizedString[] WrongAnswers => _wrongAnswers;
    public LocalizedString CorrectAnswer => _correctAnswer;
    #endregion
}
