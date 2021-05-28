using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DialogueManager : Singleton<DialogueManager>
{
    #region Fields
    #region Speech
    [Header("Speech Panel")]
    [SerializeField] private GameObject _speechPanel;
    [SerializeField] private GameObject _charactersContainer;
    [SerializeField] private TextMeshProUGUI _speechNameText;
    [SerializeField] private LocalizeStringEvent _speechText;
    [SerializeField] private Button _speechBoxButton;
    [SerializeField] private GameObject _clickToContinue;
    private Image[] _characterImages;
    #endregion

    #region Quiz
    [Header("Quiz Panel")]
    [SerializeField] private GameObject _quizPanel;
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
        reassignSpeechBoxButtonListeners();
    }

    private void Start()
    {
        _speechNameText.text = "";
        _speechText.GetComponent<TextMeshProUGUI>().text = "";

        _characterImages = new Image[_charactersContainer.transform.childCount];
        for (int i = 0; i < _charactersContainer.transform.childCount; i++)
            _characterImages[i] = _charactersContainer.transform.GetChild(i).GetComponent<Image>();
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

        bool shouldQuickComplete = false;
        UnityAction completeOnClickAction = () => completeOnClick();
        for (int i = 0; i < targetText.Length; i++)
        {
            if (i > 0)
            {
                bool hasClicked = false;

                UnityAction onClickAction = () => onClick();
                _speechBoxButton.onClick.AddListener(onClickAction);
                _clickToContinue.SetActive(true);

                while (!hasClicked)
                    yield return new WaitForEndOfFrame();

                _speechBoxButton.onClick.RemoveListener(onClickAction);
                _clickToContinue.SetActive(false);

                #region Local Methods
                void onClick()
                {
                    hasClicked = true;
                }
                #endregion
            }

            string currentText = "";
            string[] speechAndTags = targetText[i].Split(new char[2] { '<', '>' });
            for (int k = 0; k < speechAndTags.Length; k++)
            {
                string section = speechAndTags[k];
                bool isTag = (k & 1) != 0;

                if (isTag)
                {
                    currentText = speechText.text;
                    EncapsulatedText encapsulation = new EncapsulatedText(string.Format("<{0}>", section), speechAndTags, k);
                    _speechBoxButton.onClick.AddListener(completeOnClickAction);
                    while (!encapsulation.IsDone)
                    {
                        bool hasStepped = encapsulation.Step();
                        speechText.text = currentText + encapsulation.DisplayText;
                        if (hasStepped)
                            if (!shouldQuickComplete) yield return new WaitForSeconds(delay);
                    }
                    _speechBoxButton.onClick.RemoveListener(completeOnClickAction);
                    shouldQuickComplete = false;
                    k = encapsulation.SpeechAndTagsProgress + 1;
                }
                else
                {
                    _speechBoxButton.onClick.AddListener(completeOnClickAction);
                    for (int j = 0; j < section.Length; j++)
                    {
                        speechText.text += section[j];
                        if (!shouldQuickComplete) yield return new WaitForSeconds(delay);
                    }
                    _speechBoxButton.onClick.RemoveListener(completeOnClickAction);
                    shouldQuickComplete = false;
                }
            }
        }

        _speak = null;
        finishSpeak();

        #region Local Methods
        void completeOnClick()
        {
            shouldQuickComplete = true;
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
        if (_speak != null) { StopCoroutine(_speak); _speak = null; }

        _speechText.enabled = true;
        _speechText.StringReference = _currentDialogue.Lines[_dialogueProgress].LocalizedString;

        _clickToContinue.SetActive(true);

        reassignSpeechBoxButtonListeners();
    }

    private bool progressDialogue()
    {
        if (_dialogueProgress + 1 < _currentDialogue.Lines.Length) { _dialogueProgress++; return true; }
        else { endDialogue(); return false; }
    }

    private bool regressDialogue()
    {
        if (_dialogueProgress - 1 >= 0) { _dialogueProgress--; return true; }
        else return false;
    }

    private void endDialogue()
    {
        _speechPanel.SetActive(false);
    }

    private void reassignSpeechBoxButtonListeners()
    {
        _speechBoxButton.onClick.RemoveAllListeners();
        _speechBoxButton.onClick.AddListener(_nextLineAction);
    }
    #endregion

    private class EncapsulatedText
    {
        #region Fields
        private string _tag = "";
        private string _endTag = "";
        private string _currentText = "";
        private string _targetText = "";
        private string _displayText = "";
        private bool _isDone = false;
        private string[] _speechAndTags = null;
        private int _speechAndTagsProgress = 0;
        private EncapsulatedText _encapsulator = null;
        private EncapsulatedText _subEncapsulator = null;
        #endregion

        #region Properties
        public string DisplayText => _displayText;
        public bool IsDone => _isDone;
        public int SpeechAndTagsProgress => _speechAndTagsProgress;
        private EncapsulatedText Encapsulator { get => _encapsulator; set => _encapsulator = value; }
        private EncapsulatedText SubEncapsulator { get => _subEncapsulator; set => _subEncapsulator = value; }
        #endregion

        #region Methods
        public EncapsulatedText(string pTag, string[] pSpeechAndTags, int pSpeechAndTagsProgress)
        {
            _tag = pTag;
            generateEndTag();
            _speechAndTags = pSpeechAndTags;
            _speechAndTagsProgress = pSpeechAndTagsProgress;

            if (_speechAndTags.Length - 1 <= _speechAndTagsProgress) return;

            string nextPart = _speechAndTags[_speechAndTagsProgress + 1];
            _targetText = nextPart;
            _speechAndTagsProgress++;
        }

        private void generateEndTag()
        {
            _endTag = _tag.Replace("<", "").Replace(">", "");

            if (_endTag.Contains("="))
                _endTag = string.Format("</{0}>", _endTag.Split('=')[0]);
            else
                _endTag = string.Format("</{0}>", _endTag);
        }

        public bool Step()
        {
            if (_isDone) return true;
            if (_subEncapsulator != null && !_subEncapsulator.IsDone) return _subEncapsulator.Step();
            else
            {
                if (_currentText == _targetText)
                {
                    if (_speechAndTags.Length > _speechAndTagsProgress + 1)
                    {
                        string nextPart = _speechAndTags[_speechAndTagsProgress + 1];
                        bool isTag = ((_speechAndTagsProgress + 1) & 1) != 0;
                        if (isTag)
                        {
                            if (string.Format("<{0}>", nextPart) == _endTag)
                            {
                                _isDone = true;
                                if (_encapsulator != null)
                                {
                                    string taggedText = (_tag + _currentText + _endTag);
                                    _encapsulator._currentText += taggedText;
                                    _encapsulator._targetText += taggedText;
                                    updateArrayProgress(2);
                                }
                            }
                            else
                            {
                                _subEncapsulator = new EncapsulatedText(string.Format("<{0}>", nextPart), _speechAndTags, _speechAndTagsProgress + 1) { Encapsulator = this };
                                updateArrayProgress();
                            }
                        }
                        else
                        {
                            _targetText += nextPart;
                            updateArrayProgress();
                        }
                    }
                    else _isDone = true;
                }
                else
                {
                    _currentText += _targetText[_currentText.Length];
                    updateDisplay("");
                    return true;
                }
            }

            return false;
        }

        private void updateArrayProgress(int pValue = 1)
        {
            _speechAndTagsProgress += pValue;

            if (_encapsulator != null)
                _encapsulator.updateArrayProgress(pValue);
        }

        private void updateDisplay(string pSubText)
        {
            _displayText = string.Format("{0}{1}{2}{3}", _tag, _currentText, pSubText, _endTag);

            if (_encapsulator != null)
                _encapsulator.updateDisplay(_displayText);
        }
        #endregion
    }
}
