using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class DialogueManager : Singleton<DialogueManager>
{
    #region Fields
    #region Speech
    [Header("Speech Panel")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private GameObject _speechPanel;
    [SerializeField] private GameObject _charactersContainer;
    [SerializeField] private TextMeshProUGUI _speechNameText;
    [SerializeField] private LocalizeStringEvent _speechLocalizedStingEvent;
    [SerializeField] private LocalizeStringEvent _hintLocalizedStringEvent;
    [SerializeField] private Button _speechBoxButton;
    [SerializeField] private GameObject _clickToContinue;

    private Image[] _characterImages;
    #endregion

    #region Quiz
    public enum GameType
    {
        Quiz,
        AR
    }
    [Header("Gameplay")]
    [SerializeField] private GameType _typeOfGame;
    [DrawIf("_typeOfGame", GameType.Quiz)] [SerializeField] private GameObject _quizPanel;
    [DrawIf("_typeOfGame", GameType.AR)] [SerializeField] private GameObject _arPanel;
    #endregion

    #region Dialogue
    private DialogueScriptableObject _currentDialogue;
    private int _dialogueProgress;
    private Coroutine _speak = null;
    private Coroutine _speakHint = null;
    #endregion

    private UnityAction _nextLineAction;
    #endregion

    #region Properties
    #endregion

    #region Methods
    private void Awake()
    {
        _nextLineAction = () => nextLine();
        _speechPanel.SetActive(false);
        reassignSpeechBoxButtonListeners();
    }

    private void Start()
    {
        _speechNameText.text = "";
        _speechLocalizedStingEvent.GetComponent<TextMeshProUGUI>().text = "";

        _characterImages = new Image[_charactersContainer.transform.childCount];
        for (int i = 0; i < _charactersContainer.transform.childCount; i++)
            _characterImages[i] = _charactersContainer.transform.GetChild(i).GetComponent<Image>();
    }

    public void StartNewDialogue(DialogueScriptableObject pDialogue, int pDialogueProgress = 0)
    {
        _speechPanel.SetActive(true);
        _currentDialogue = pDialogue;
        _dialogueProgress = pDialogueProgress - 1;
        if (_speak != null) { StopCoroutine(_speak); _speak = null; }
        nextLine();
    }

    public void Resume()
    {
        reassignSpeechBoxButtonListeners();
        nextLine();
    }

    public void Repeat()
    {
        reassignSpeechBoxButtonListeners();
        sameLine();
    }

    private void previousLine()
    {
        //If _speak is running stop it
        if (_speak != null) { finishSpeak(); return; }

        regressDialogue();

        _speechLocalizedStingEvent.StringReference = prepareLine().LineString;
    }

    private void sameLine()
    {
        //If _speak is running stop it
        if (_speak != null) { finishSpeak(); return; }

        //Say the line
        _speechLocalizedStingEvent.gameObject.SetActive(true);
        _hintLocalizedStringEvent.gameObject.SetActive(false);
        _speak = StartCoroutine(speak(prepareLine()));
    }

    private void nextLine()
    {
        //If _speak is running stop it
        if (_speak != null) { finishSpeak(); return; }

        //Check if dialogue has finished
        if (!progressDialogue()) return;

        //Say the line
        _speechLocalizedStingEvent.gameObject.SetActive(true);
        _hintLocalizedStringEvent.gameObject.SetActive(false);
        _speak = StartCoroutine(speak(prepareLine()));
    }

    private Line prepareLine()
    {
        Line line = _currentDialogue.Lines[_dialogueProgress];
        for (int i = 0; i < _characterImages.Length; i++)
        {
            if (i >= line.LineCharacters.Length) { _characterImages[i].gameObject.SetActive(false); continue; }

            if (line.LineCharacters[i].CharacterScriptableObject == null)
            {
                Debug.LogWarning("LineCharacter of index " + i + " in the dialogue " + _currentDialogue.name + " is not assigned.");
                _characterImages[i].gameObject.SetActive(false);
                continue;
            }

            if (line.LineCharacters[i].CharacterScriptableObject.name == "Player")
            {
                _characterImages[i].gameObject.SetActive(false);
                continue;
            }

            _characterImages[i].gameObject.SetActive(true);
            _characterImages[i].sprite = line.GetCharacterExpression(i);
        }
        if (line.BackgroundSprite != null) _backgroundImage.sprite = line.BackgroundSprite;
        _speechNameText.text = line.GetSpeaker().CharacterScriptableObject.Name;
        _clickToContinue.SetActive(false);
        return line;
    }

    private IEnumerator speak(Line pLine)
    {
        _speechBoxButton.onClick.RemoveAllListeners();

        float delay = 1.0f / pLine.TextCharactersPerSecond;

        TextMeshProUGUI speechText = _speechLocalizedStingEvent.GetComponent<TextMeshProUGUI>();

        speechText.text = "";
        _speechLocalizedStingEvent.enabled = false;

        //var operation = pLine.LineString.GetLocalizedString();
        //while (!operation.IsDone)
        //    yield return null;
        //string[] targetText = operation.Result.Split(new string[] { "<size=0>|</size>" }, StringSplitOptions.None);
        string[] targetText = pLine.LineString.GetLocalizedString().Split(new string[] { "<size=0>|</size>" }, StringSplitOptions.None);
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

        finishSpeak();

        #region Local Methods
        void completeOnClick()
        {
            shouldQuickComplete = true;
        }
        #endregion
    }

    private void finishSpeak()
    {
        Line line = _currentDialogue.Lines[_dialogueProgress];

        StopCoroutine(_speak);
        _speak = null;

        _speechLocalizedStingEvent.enabled = true;
        _speechLocalizedStingEvent.StringReference = line.LineString;

        _speechBoxButton.onClick.RemoveAllListeners();

        if (_typeOfGame == GameType.Quiz && line.QuizQuestions.Length > 0)
            StartCoroutine(waitForInputBeforeAction(() => QuizManager.Instance.PrepareQuestions(line.QuizQuestions)));
        else if (_typeOfGame == GameType.AR && line.ImagesToScan.Length > 0)
            StartCoroutine(waitForInputBeforeAction(() => ARManager.Instance.PrepareImagesToScan(line.ImagesToScan)));
        else
        {
            _speechBoxButton.onClick.AddListener(_nextLineAction);
            _clickToContinue.SetActive(true);
        }
    }

    public void ShowHint(LocalizedString pString)
    {
        if (_speakHint != null)
            StopCoroutine(_speakHint);

        _speechLocalizedStingEvent.gameObject.SetActive(false);
        _hintLocalizedStringEvent.gameObject.SetActive(true);
        _speakHint = StartCoroutine(speakHint(pString));
    }

    public void HideHint()
    {
        if (_speakHint != null)
            StopCoroutine(_speakHint);
        _speakHint = null;

        _speechLocalizedStingEvent.gameObject.SetActive(true);
        _hintLocalizedStringEvent.gameObject.SetActive(false);
    }

    private IEnumerator speakHint(LocalizedString pString)
    {
        _speechBoxButton.onClick.RemoveAllListeners();

        float delay = 1.0f / 50;

        TextMeshProUGUI hintText = _hintLocalizedStringEvent.GetComponent<TextMeshProUGUI>();
        _hintLocalizedStringEvent.enabled = false;
        hintText.text = "";

        //var operation = pString.GetLocalizedString();
        //while (!operation.IsDone)
        //    yield return null;
        //string targetText = operation.Result;
        string targetText = pString.GetLocalizedString();

        string[] speechAndTags = targetText.Split(new char[2] { '<', '>' });
        for (int i = 0; i < speechAndTags.Length; i++)
        {
            string section = speechAndTags[i];
            bool isTag = (i & 1) != 0;

            if (isTag)
            {
                string currentText = hintText.text;
                EncapsulatedText encapsulation = new EncapsulatedText(string.Format("<{0}>", section), speechAndTags, i);
                while (!encapsulation.IsDone)
                {
                    bool hasStepped = encapsulation.Step();
                    hintText.text = currentText + encapsulation.DisplayText;
                    if (hasStepped)
                        yield return new WaitForSeconds(delay);
                }
                i = encapsulation.SpeechAndTagsProgress + 1;
            }
            else
            {
                for (int j = 0; j < section.Length; j++)
                {
                    hintText.text += section[j];
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        finishSpeakHint(pString);
    }

    private void finishSpeakHint(LocalizedString pString)
    {
        StopCoroutine(_speakHint);
        _speakHint = null;

        _hintLocalizedStringEvent.enabled = true;
        _hintLocalizedStringEvent.StringReference = pString;
    }

    private IEnumerator waitForInputBeforeAction(Action pAction)
    {
        bool hasClicked = false;

        UnityAction onClickAction = () => onClick();
        _speechBoxButton.onClick.AddListener(onClickAction);
        _clickToContinue.SetActive(true);

        while (!hasClicked)
            yield return new WaitForEndOfFrame();

        _speechBoxButton.onClick.RemoveListener(onClickAction);
        _clickToContinue.SetActive(false);

        pAction();

        #region Local Methods
        void onClick()
        {
            hasClicked = true;
        }
        #endregion
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
