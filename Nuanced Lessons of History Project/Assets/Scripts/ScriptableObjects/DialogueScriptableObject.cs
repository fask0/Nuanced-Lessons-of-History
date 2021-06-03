using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class LineCharacter
{
    #region Fields
    [SerializeField] private CharacterScriptableObject _characterScriptableObject;
    [SerializeField] private ExpressionType _expression;
    [SerializeField] private bool _isSpeaker;
    #endregion

    #region Properties
    public CharacterScriptableObject CharacterScriptableObject => _characterScriptableObject;
    public ExpressionType Expression => _expression;
    public bool IsSpeaker => _isSpeaker;
    #endregion
}

[System.Serializable]
public class Line
{
    #region Fields
    [SerializeField] private LineCharacter[] _lineCharacters;
    [SerializeField] private LocalizedString _lineString;
    [Range(10, 60)] [SerializeField] private int _textCharactersPerSecond = 30;
    [SerializeField] private Sprite _backgroundSprite = null;

    [Header("Quiz")]
    [SerializeField] private QuestionScriptableObject[] _questions;
    [Header("AR")]
    [SerializeField] private ScannableImageScriptableObject[] _imagesToScan;
    #endregion

    #region Properties
    public LineCharacter[] LineCharacters => _lineCharacters;
    public LocalizedString LineString => _lineString;
    public int TextCharactersPerSecond => (_textCharactersPerSecond == 0) ? 30 : _textCharactersPerSecond;
    public Sprite BackgroundSprite => _backgroundSprite;
    public QuestionScriptableObject[] Questions => _questions;
    public ScannableImageScriptableObject[] ImagesToScan => _imagesToScan;
    #endregion

    #region Methods
    public Sprite[] GetAllCharacterExpressions()
    {
        Sprite[] expressions = new Sprite[_lineCharacters.Length];

        for (int i = 0; i < _lineCharacters.Length; i++)
            expressions[i] = _lineCharacters[i].CharacterScriptableObject.GetExpression(_lineCharacters[i].Expression);

        return expressions;
    }

    public Sprite GetCharacterExpression(int pLineCharacterIndex)
    {
        return _lineCharacters[pLineCharacterIndex].CharacterScriptableObject.GetExpression(_lineCharacters[pLineCharacterIndex].Expression);
    }

    public Sprite GetCharacterExpression(LineCharacter pLineCharacter)
    {
        return pLineCharacter.CharacterScriptableObject.GetExpression(pLineCharacter.Expression);
    }

    public int GetSpeakerIndex()
    {
        for (int i = 0; i < _lineCharacters.Length; i++)
            if (_lineCharacters[i].IsSpeaker) return i;

        return -1;
    }

    public LineCharacter GetSpeaker()
    {
        for (int i = 0; i < _lineCharacters.Length; i++)
            if (_lineCharacters[i].IsSpeaker) return _lineCharacters[i];

        return null;
    }
    #endregion
}

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private Line[] _lines;
    #endregion

    #region Properties
    public Line[] Lines => _lines;
    #endregion
}
