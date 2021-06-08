using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "QQ_QuizQuestion", menuName = "Scriptable Objects/QuizQuestion")]
public class QuizQuestionScriptableObject : QuestionScriptableObject
{
    #region Fields
    [SerializeField] protected LocalizedString _correctAnswer;
    [HorizontalLine(1)]
    [SerializeField] private SpecialAction[] _onAnswerActions;
    [SerializeField] private SpecialAction _onCorrectAnswerAction;
    [HorizontalLine(1)]
    [SerializeField] protected LocalizedString[] _hints;
    #endregion

    #region Properties
    public LocalizedString CorrectAnswer => _correctAnswer;
    public SpecialAction[] OnAnswerActions => _onAnswerActions;
    public SpecialAction OnCorrectAnswerAction => _onCorrectAnswerAction;
    public LocalizedString[] Hints => _hints;
    #endregion

    #region Methods
    private void OnValidate()
    {
        if (_answers == null || _answers.Length == 0)
            _answers = new LocalizedString[1];
        if (_onAnswerActions == null)
            _onAnswerActions = new SpecialAction[1];

        int compareVal = _onAnswerActions.Length.CompareTo(_answers.Length);
        if (compareVal == 0) return;
        SpecialAction[] tempArray = new SpecialAction[_answers.Length];
        for (int i = 0; i < ((compareVal == -1) ? _onAnswerActions.Length : _answers.Length); i++)
            tempArray[i] = _onAnswerActions[i];
        _onAnswerActions = tempArray;
    }
    #endregion
}
