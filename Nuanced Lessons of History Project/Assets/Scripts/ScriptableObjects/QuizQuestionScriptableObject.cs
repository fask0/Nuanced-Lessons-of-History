using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

[CreateAssetMenu(fileName = "QuizQuestion", menuName = "Scriptable Objects/QuizQuestion")]
public class QuizQuestionScriptableObject : QuestionScriptableObject
{
    #region Fields
    [SerializeField] protected LocalizedString _correctAnswer;
    [SerializeField] protected LocalizedString[] _hints;
    #endregion

    #region Properties
    public LocalizedString CorrectAnswer => _correctAnswer;
    public LocalizedString[] Hints => _hints;
    #endregion
}
