using NaughtyAttributes;
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
    [HorizontalLine(1)]
    [Header("Speech Panel")]
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private GameObject _speechPanel;
    [SerializeField] private GameObject _charactersContainer;
    [SerializeField] private LocalizeStringEvent _speechNameLocalizedStingEvent;
    [SerializeField] private LocalizeStringEvent _speechLocalizedStingEvent;
    [SerializeField] private LocalizeStringEvent _hintLocalizedStringEvent;
    [SerializeField] private Button _speechBoxButton;
    [SerializeField] private GameObject _clickToContinue;
    [HorizontalLine(1)]
    [Header("Other Panels")]
    [SerializeField] private GameObject _quizPanel;
    [SerializeField] private GameObject _arPanel;

    private Image[] _characterImages;
    private DialogueScriptableObject _currentDialogue;
    private int _dialogueProgress;
    private Coroutine _speak = null;
    private Coroutine _speakHint = null;
    private UnityAction _nextLineAction;
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
        _speechNameLocalizedStingEvent.GetComponent<TextMeshProUGUI>().text = "";
        _speechNameLocalizedStingEvent.enabled = false;
        _speechLocalizedStingEvent.GetComponent<TextMeshProUGUI>().text = "";

        _characterImages = new Image[_charactersContainer.transform.childCount];
        for (int i = 0; i < _charactersContainer.transform.childCount; i++)
            _characterImages[i] = _charactersContainer.transform.GetChild(i).GetComponent<Image>();
    }

    public void StartNewDialogue(DialogueScriptableObject pDialogue, int pDialogueProgress = 0)
    {
        //Make a clone so that the original SO never gets changed
        _currentDialogue = Instantiate(pDialogue);
        _dialogueProgress = pDialogueProgress - 1;

        _speechPanel.SetActive(true);

        if (_speak != null) { StopCoroutine(_speak); _speak = null; }

        reassignSpeechBoxButtonListeners();
        nextLine();
    }

    public void AddNewLine(Line pLine)
    {
        if (pLine.LineString == null) return;

        Line[] newLineArray = new Line[_currentDialogue.Lines.Length + 1];
        for (int i = 0; i < newLineArray.Length; i++)
        {
            if (i < _dialogueProgress + 1)
                newLineArray[i] = _currentDialogue.Lines[i];
            else if (i == _dialogueProgress + 1)
                newLineArray[i] = pLine;
            else
                newLineArray[i] = _currentDialogue.Lines[i - 1];
        }
        _currentDialogue.Lines = newLineArray;
    }

    public void OverrideNextLine(Line pLine)
    {
        if (pLine.LineString == null || _dialogueProgress + 1 >= _currentDialogue.Lines.Length) return;
        _currentDialogue.Lines[_dialogueProgress + 1] = pLine;
    }

    public void Resume()
    {
        _quizPanel.SetActive(false);
        _arPanel.SetActive(false);
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

            LineCharacter lineCharacter = line.LineCharacters[i];

            if (!lineCharacter.HasCharacter)
            {
                Debug.LogWarning("LineCharacter of index " + i + " in the dialogue " + _currentDialogue.name + " is not assigned.");
                _characterImages[i].gameObject.SetActive(false);
                continue;
            }

            Sprite characterExpression = line.GetCharacterExpression(lineCharacter);
            if (lineCharacter.HideCharacterImage || characterExpression == null)
            {
                _characterImages[i].gameObject.SetActive(false);
                continue;
            }
            _characterImages[i].gameObject.SetActive(true);
            _characterImages[i].sprite = characterExpression;
        }

        LineCharacter speaker = line.GetSpeaker();
        if (speaker.CharacterNameIsLocalized)
        {
            _speechNameLocalizedStingEvent.StringReference = speaker.GetLocalizedName();
            _speechNameLocalizedStingEvent.enabled = true;
        }
        else
        {
            _speechNameLocalizedStingEvent.GetComponent<TextMeshProUGUI>().text = speaker.GetName();
            _speechNameLocalizedStingEvent.enabled = false;
        }
        _clickToContinue.SetActive(false);

        Canvas.ForceUpdateCanvases();
        _speechNameLocalizedStingEvent.gameObject.SetActive(false);
        _speechNameLocalizedStingEvent.gameObject.SetActive(true);

        if (line.BackgroundSprite != null) _backgroundImage.sprite = line.BackgroundSprite;

        return line;
    }

    private IEnumerator speak(Line pLine)
    {
        _speechBoxButton.onClick.RemoveAllListeners();

        if (pLine.SoundBeforeLine != null)
        {
            SoundManager.Instance.PlayNewSoundClip(pLine.SoundBeforeLine);
            UnityAction onClickAction = () => skipWaitOnClick();
            _speechBoxButton.onClick.AddListener(onClickAction);
            float timeRemaining = pLine.SoundBeforeLine.length;
            while (pLine.WaitUntilSoundBeforeLineEnds)
            {
                yield return new WaitForEndOfFrame();
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0) break;
            }
            _speechBoxButton.onClick.RemoveListener(onClickAction);

            #region Local Methods
            void skipWaitOnClick()
            {
                timeRemaining = 0;
                //SoundManager.Instance.StopSound();
            }
            #endregion
        }

        float delay = 1.0f / pLine.TextCharactersPerSecond;

        TextMeshProUGUI speechText = _speechLocalizedStingEvent.GetComponent<TextMeshProUGUI>();

        speechText.text = "";
        _speechLocalizedStingEvent.enabled = false;

        string[] targetText = (pLine.LineString != null) ? pLine.LineString.GetLocalizedString().Split(new string[] { "<size=0>|</size>" }, StringSplitOptions.None) : new string[1] { "" };
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

        if (pLine.SoundAfterLine != null)
        {
            SoundManager.Instance.PlayNewSoundClip(pLine.SoundAfterLine);
            UnityAction onClickAction = () => stopSoundOnClick();
            _speechBoxButton.onClick.AddListener(onClickAction);
            float timeRemaining = pLine.SoundBeforeLine.length;
            while (pLine.WaitUntilSoundAfterLineEnds)
            {
                yield return new WaitForEndOfFrame();
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0) break;
            }
            _speechBoxButton.onClick.RemoveListener(onClickAction);

            #region Local Methods
            void stopSoundOnClick()
            {
                timeRemaining = 0;
                SoundManager.Instance.StopSound();
            }
            #endregion
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

        if (line.LineString != null)
        {
            _speechLocalizedStingEvent.enabled = true;
            _speechLocalizedStingEvent.StringReference = line.LineString;
        }

        _speechBoxButton.onClick.RemoveAllListeners();

        if (line.Interaction != Line.PlayerInteraction.None)
            StartCoroutine(waitForInputBeforeAction(() => line.HandlePlayerInteraction()));
        else
        {
            line.HandlePlayerInteraction();
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
