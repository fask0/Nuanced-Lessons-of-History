using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Video;

[System.Serializable]
public class SpecialAction
{
    public enum Action
    {
        None,
        StartNewDialogue,
        AddNewLine,
        OverrideNextLine,
        QuizQuestion,
        StoryQuestion,
        ImageToScan,
        PlayVideoClip
    }

    #region Fields
    [SerializeField] private Action _action;
    [AllowNesting] [ShowIf("_action", Action.StartNewDialogue)] [SerializeField] private DialogueScriptableObject _newDialogue;
    [AllowNesting] [ShowIf("EnableNewLine")] [SerializeField] private Line _nextLine;
    [AllowNesting] [ShowIf("_action", Action.QuizQuestion)] [SerializeField] private QuizQuestionScriptableObject _quizQuestion;
    [AllowNesting] [ShowIf("_action", Action.StoryQuestion)] [SerializeField] private StoryQuestionScriptableObject _storyQuestion;
    [AllowNesting] [ShowIf("_action", Action.ImageToScan)] [SerializeField] private ScannableImageScriptableObject _imageToScan;
    [AllowNesting] [ShowIf("_action", Action.PlayVideoClip)] [SerializeField] private VideoClip _videoClip;
    [AllowNesting] [ShowIf("_action", Action.PlayVideoClip)] [SerializeField] private float _videoClipStartDelay;

    private bool EnableNewLine => _action == Action.AddNewLine || _action == Action.OverrideNextLine;
    #endregion

    #region Properties
    public Action ActionType => _action;
    #endregion

    public void HandleAction()
    {
        switch (_action)
        {
            case Action.None:
                break;
            case Action.StartNewDialogue:
                DialogueManager.Instance.StartNewDialogue(_newDialogue);
                break;
            case Action.AddNewLine:
                DialogueManager.Instance.AddNewLine(_nextLine);
                DialogueManager.Instance.Resume();
                break;
            case Action.OverrideNextLine:
                DialogueManager.Instance.OverrideNextLine(_nextLine);
                DialogueManager.Instance.Resume();
                break;
            case Action.QuizQuestion:
                QuizManager.Instance.PrepareQuestion(_quizQuestion);
                break;
            case Action.StoryQuestion:
                QuizManager.Instance.PrepareQuestion(_storyQuestion);
                break;
            case Action.ImageToScan:
                ARManager.Instance.PrepareImageToScan(_imageToScan);
                break;
            case Action.PlayVideoClip:
                VideoManager.Instance.PlayNewClip(_videoClip, _videoClipStartDelay);
                break;
            default:
                break;
        }
    }
}

public class QuestionScriptableObject : ScriptableObject
{
    #region Fields
    [SerializeField] protected LocalizedString _question;
    [HorizontalLine(1)]
    [SerializeField] protected LocalizedString[] _answers = new LocalizedString[1];
    #endregion

    #region Properties
    public LocalizedString Question => _question;
    public LocalizedString[] Answers => _answers;
    #endregion
}
