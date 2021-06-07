using UnityEngine;
using UnityEngine.Localization;
using NaughtyAttributes;

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
    public enum PlayerInteraction
    {
        None,
        QuizQuestion,
        StoryQuestion,
        ImageToScan
    }

    #region Fields
    [SerializeField] private LineCharacter[] _lineCharacters;
    [HorizontalLine(1)]
    [SerializeField] private LocalizedString _lineString;
    [Range(10, 60)] [SerializeField] private int _textCharactersPerSecond = 30;
    [HorizontalLine(1)]
    [SerializeField] private Sprite _backgroundSprite = null;
    [HorizontalLine(1)]
    [SerializeField] private PlayerInteraction _playerInteraction;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.QuizQuestion)] [SerializeField] private QuizQuestionScriptableObject _quizQuestion;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.StoryQuestion)] [SerializeField] private StoryQuestionScriptableObject _storyQuestion;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.ImageToScan)] [SerializeField] private ScannableImageScriptableObject _imageToScan;
    #endregion

    #region Properties
    public LineCharacter[] LineCharacters => _lineCharacters;
    public LocalizedString LineString => _lineString;
    public int TextCharactersPerSecond => (_textCharactersPerSecond == 0) ? 30 : _textCharactersPerSecond;
    public Sprite BackgroundSprite => _backgroundSprite;
    public PlayerInteraction Interaction => _playerInteraction;
    #endregion

    #region Methods
    public void HandlePlayerInteraction()
    {
        switch (_playerInteraction)
        {
            case PlayerInteraction.None:
                break;
            case PlayerInteraction.QuizQuestion:
                QuizManager.Instance.PrepareQuestion(_quizQuestion);
                break;
            case PlayerInteraction.StoryQuestion:
                QuizManager.Instance.PrepareQuestion(_storyQuestion);
                break;
            case PlayerInteraction.ImageToScan:
                //ARManager.Instance.PrepareImagesToScan(_imageToScan);
                break;
        }
    }

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

[CreateAssetMenu(fileName = "Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private Line[] _lines;
    #endregion

    #region Properties
    public Line[] Lines { get => _lines; set => _lines = value; }
    #endregion
}
