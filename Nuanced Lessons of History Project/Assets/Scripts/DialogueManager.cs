using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DialogueManager : Singleton<DialogueManager>
{
    #region Fields
    #region Speech Panel
    [Header("Speech Panel")]
    [SerializeField] private GameObject _speechPanel;
    [SerializeField] private Image[] _characterImages;
    [SerializeField] private TextMeshProUGUI _speechNameText;
    [SerializeField] private LocalizeStringEvent _speechText;
    [SerializeField] private Button _speechBoxButton;
    [SerializeField] private GameObject _clickToContinue;
    #endregion

    #region Dialogue
    private DialogueScriptableObject _currentDialogue;
    private int _dialogueProgress;
    private Coroutine _speak = null;
    #endregion

    private UnityAction _nextLineAction;
    #endregion

    #region Properties
    #endregion

    #region Methods
    private void Awake()
    {
        _nextLineAction = () => NextLine();
        _speechPanel.SetActive(false);
        _speechBoxButton.onClick.RemoveAllListeners();
        _speechBoxButton.onClick.AddListener(_nextLineAction);
    }

    private void Start()
    {
        _speechNameText.text = "";
        _speechText.GetComponent<TextMeshProUGUI>().text = "";
    }

    public void StartNewDialogue(DialogueScriptableObject pDialogue)
    {
        _speechPanel.SetActive(true);
        _currentDialogue = pDialogue;
        _dialogueProgress = -1;
        NextLine();
    }

    public void PreviousLine()
    {
        //If _speak is running stop it
        if (_speak != null) { finishSpeak(); return; }

        regressDialogue();

        Line line = prepareLine();
        _speechText.StringReference = line.LocalizedString;
    }

    public void NextLine()
    {
        //If _speak is running stop it
        if (_speak != null) { finishSpeak(); return; }

        //Check if dialogue has finished
        if (!progressDialogue()) return;

        //Say the line
        Line line = prepareLine();
        _speak = StartCoroutine(speak(line));
    }

    private IEnumerator speak(Line pLine = null)
    {
        _speechBoxButton.onClick.RemoveAllListeners();

        float delay = 1.0f / pLine.TextCharactersPerSecond;

        TextMeshProUGUI speechText = _speechText.GetComponent<TextMeshProUGUI>();

        speechText.text = "";
        _speechText.enabled = false;

        var operation = pLine.LocalizedString.GetLocalizedString();
        while (!operation.IsDone)
            yield return null;
        string[] targetText = operation.Result.Split(new string[] { "<size=0>|</size>" }, System.StringSplitOptions.None);

        if (targetText.Length <= 1)
            _speechBoxButton.onClick.AddListener(_nextLineAction);

        bool wantToQuickComplete = false;
        UnityAction completeOnClickAction = () => completeOnClick();
        for (int i = 0; i < targetText.Length; i++)
        {
            if (i > 0)
            {
                bool hasClicked = false;

                UnityAction onClickAction = () => onClick();
                _speechBoxButton.onClick.AddListener(onClickAction);

                while (!hasClicked)
                    yield return new WaitForEndOfFrame();

                _speechBoxButton.onClick.RemoveListener(onClickAction);

                #region Local Methods
                void onClick()
                {
                    hasClicked = true;
                }
                #endregion
            }

            _speechBoxButton.onClick.AddListener(completeOnClickAction);
            for (int k = 0; k < targetText[i].Length; k++)
            {
                speechText.text += targetText[i][k];
                if (!wantToQuickComplete) yield return new WaitForSeconds(delay);
            }
            _speechBoxButton.onClick.RemoveListener(completeOnClickAction);
            wantToQuickComplete = false;
        }

        finishSpeak();

        #region Local Methods
        void completeOnClick()
        {
            wantToQuickComplete = true;
        }
        #endregion
    }

    private Line prepareLine()
    {
        Line line = _currentDialogue.Lines[_dialogueProgress];
        for (int i = 0; i < _characterImages.Length; i++)
        {
            if (i >= line.LineCharacters.Length) { _characterImages[i].gameObject.SetActive(false); continue; }
            if (line.LineCharacters[i].CharacterScriptableObject.name == "Player") { _characterImages[i].gameObject.SetActive(false); continue; }

            _characterImages[i].gameObject.SetActive(true);
            _characterImages[i].sprite = line.GetCharacterExpression(i);
        }
        _speechNameText.text = line.GetSpeaker().CharacterScriptableObject.Name;
        _clickToContinue.SetActive(false);
        return line;
    }

    private void finishSpeak()
    {
        StopCoroutine(_speak);

        _speechText.enabled = true;
        _speechText.StringReference = _currentDialogue.Lines[_dialogueProgress].LocalizedString;

        _clickToContinue.SetActive(true);

        _speak = null;

        _speechBoxButton.onClick.RemoveAllListeners();
        _speechBoxButton.onClick.AddListener(_nextLineAction);
    }

    private bool progressDialogue()
    {
        if (_dialogueProgress + 1 < _currentDialogue.Lines.Length)
        {
            _dialogueProgress++;
            return true;
        }
        else
        {
            //End of dialogue
            endDialogue();
            return false;
        }
    }

    private void regressDialogue()
    {
        if (_dialogueProgress - 1 >= 0)
            _dialogueProgress--;
    }

    private void endDialogue()
    {
        _speechPanel.SetActive(false);
    }
    #endregion
}
