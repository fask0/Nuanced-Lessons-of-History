using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "QS_StoryQuestion", menuName = "Scriptable Objects/StoryQuestion")]
public class StoryQuestionScriptableObject : QuestionScriptableObject
{
    #region Fields
    [HorizontalLine(1)]
    [SerializeField] private SpecialAction[] _onAnswerActions;
    #endregion

    #region Properties
    public SpecialAction[] OnAnswerActions => _onAnswerActions;
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
