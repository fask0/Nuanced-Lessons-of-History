using UnityEngine;
using UnityEngine.Localization;
using NaughtyAttributes;
using UnityEngine.Video;

[System.Serializable]
public class LineCharacter
{
    #region Fields
    [SerializeField] private CharacterScriptableObject _characterScriptableObject;
    [SerializeField] private CharacterScriptableObject.CharacterNameToUse _nameToUse;
    [HorizontalLine(1)]
    [SerializeField] private Expression.ExpressionType _expression;
    [SerializeField] private bool _hideCharacterImage = false;
    [HorizontalLine(1)]
    [SerializeField] private bool _isSpeaker;
    #endregion

    #region Properties
    public bool HasCharacter => _characterScriptableObject != null;
    public bool HideCharacterImage => _hideCharacterImage;
    public bool IsSpeaker => _isSpeaker;
    public bool CharacterNameIsLocalized => _characterScriptableObject.IsNameLocalized;
    #endregion

    #region Methods
    public Sprite GetExpression()
    {
        return _characterScriptableObject.GetExpression(_expression);
    }

    public LocalizedString GetLocalizedName()
    {
        return _characterScriptableObject.GetLocalizedName(_nameToUse);
    }

    public string GetName()
    {
        return _characterScriptableObject.GetName(_nameToUse);
    }
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
        ImageToScan,
        StartNewDialogue,
        PlayVideoClip,
        TimePass
    }

    #region Fields
    [Header("Characters")]
    [SerializeField] private LineCharacter[] _lineCharacters;
    [HorizontalLine(1)]
    [Header("Before Text")]
    [SerializeField] private SFX[] _sfxBeforeLine;
    [HorizontalLine(1)]
    [Header("Text")]
    [SerializeField] private LocalizedString _lineString;
    [Range(10, 60)] [SerializeField] private int _textCharactersPerSecond = 30;
    [HorizontalLine(1)]
    [Header("After Text")]
    [SerializeField] private SFX[] _sfxAfterLine;
    [HorizontalLine(1)]
    [Header("Override")]
    [SerializeField] private AudioClip _ambientSoundToStartPlaying;
    [SerializeField] private Sprite _backgroundSprite = null;
    [HorizontalLine(1)]
    [Header("Interaction")]
    [SerializeField] private PlayerInteraction _playerInteraction;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.QuizQuestion)] [SerializeField] private QuizQuestionScriptableObject _quizQuestion;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.StoryQuestion)] [SerializeField] private StoryQuestionScriptableObject _storyQuestion;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.ImageToScan)] [SerializeField] private ScannableImageScriptableObject _imageToScan;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.StartNewDialogue)] [SerializeField] private DialogueScriptableObject _newDialogue;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.PlayVideoClip)] [SerializeField] private VideoClip _videoClip;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.PlayVideoClip)] [SerializeField] private float _videoClipStartDelay;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.TimePass)] [SerializeField] private SFX[] _timePassSFX;
    [AllowNesting] [ShowIf("_playerInteraction", PlayerInteraction.TimePass)] [SerializeField] private DialogueScriptableObject _newDialogueAfterTimePass;
    #endregion

    #region Properties
    public SFX[] SFXBeforeLine => _sfxBeforeLine;
    public LineCharacter[] LineCharacters => _lineCharacters;
    public LocalizedString LineString => _lineString;
    public SFX[] SFXAfterLine => _sfxAfterLine;
    public int TextCharactersPerSecond => (_textCharactersPerSecond == 0) ? 30 : _textCharactersPerSecond;
    public AudioClip AmbientSoundToStartPlaying => _ambientSoundToStartPlaying;
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
                ARManager.Instance.PrepareImageToScan(_imageToScan);
                break;
            case PlayerInteraction.StartNewDialogue:
                DialogueManager.Instance.StartNewDialogue(_newDialogue);
                break;
            case PlayerInteraction.PlayVideoClip:
                VideoManager.Instance.PlayNewClip(_videoClip, _videoClipStartDelay);
                break;
            case PlayerInteraction.TimePass:
                DialogueManager.Instance.StartNewDialogueAfterTimePass(_timePassSFX, _newDialogueAfterTimePass);
                break;
            default:
                break;
        }
    }

    public Sprite[] GetAllCharacterExpressions()
    {
        Sprite[] expressions = new Sprite[_lineCharacters.Length];

        for (int i = 0; i < _lineCharacters.Length; i++)
            expressions[i] = _lineCharacters[i].GetExpression();

        return expressions;
    }

    public Sprite GetCharacterExpression(LineCharacter pLineCharacter)
    {
        return pLineCharacter.GetExpression();
    }

    public LineCharacter GetSpeaker()
    {
        for (int i = 0; i < _lineCharacters.Length; i++)
            if (_lineCharacters[i].IsSpeaker) return _lineCharacters[i];

        Debug.LogError(string.Format("Line: {0}, with {1} characters does not have a speaker!", _lineString.GetLocalizedString(), _lineCharacters.Length));
        return null;
    }
    #endregion
}

[CreateAssetMenu(fileName = "D_Dialogue", menuName = "Scriptable Objects/Dialogue")]
public class DialogueScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] private Line[] _lines;
    #endregion

    #region Properties
    public Line[] Lines { get => _lines; set => _lines = value; }
    #endregion
}
