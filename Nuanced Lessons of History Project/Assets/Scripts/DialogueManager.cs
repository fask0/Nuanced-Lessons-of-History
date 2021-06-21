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
    [SerializeField] private GameObject _timePassPanel;
    [HorizontalLine(1)]
    [Header("Misc")]
    [SerializeField] private DialogueScriptableObject _startOfGameDialogue;
    [SerializeField] private string _specialWordColor;
    [SerializeField] private string _specialWordColorHint;
    public string c => _specialWordColor;
    public string ch => _specialWordColorHint;

    private Image[] _characterImages;
    private DialogueScriptableObject _currentDialogue;
    private int _dialogueProgress;
    private Coroutine _speak = null;
    private Coroutine _speakHint = null;
    private Coroutine _passingTime = null;
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

    public void StartNewGame()
    {
        StartNewDialogue(_startOfGameDialogue);
    }

    public void StartNewDialogueAfterTimePass(SFX[] pSFX, DialogueScriptableObject pDialogue, int pDialogueProgress = 0)
    {
        if (_passingTime != null) StopCoroutine(_passingTime);
        _passingTime = StartCoroutine(passTime(pSFX, pDialogue, pDialogueProgress));
    }

    private IEnumerator passTime(SFX[] pSFX, DialogueScriptableObject pDialogue, int pDialogueProgress = 0)
    {
        _timePassPanel.SetActive(true);

        yield return handleSFX(pSFX, _timePassPanel.GetComponent<Button>());

        _timePassPanel.SetActive(false);

        StartNewDialogue(pDialogue, pDialogueProgress);
    }

    public void StartNewDialogue(DialogueScriptableObject pDialogue, int pDialogueProgress = 0)
    {
        //Make a clone so that the original SO never gets changed
        _currentDialogue = Instantiate(pDialogue);
        _dialogueProgress = pDialogueProgress - 1;

        //Make sure the VNPanel is enabled, if it was disabled for some reason
        _speechPanel.transform.parent.gameObject.SetActive(true);
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
            //Disable any extra images
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

        //Handle normal and localized names
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

        //Force update the canvas, so that the speaker name box is the correct size
        Canvas.ForceUpdateCanvases();
        _speechNameLocalizedStingEvent.gameObject.SetActive(false);
        _speechNameLocalizedStingEvent.gameObject.SetActive(true);

        //Disregard fuctions if they are not set
        if (line.BackgroundSprite != null) _backgroundImage.sprite = line.BackgroundSprite;
        if (line.AmbientSoundToStartPlaying != null) SoundManager.Instance.PlayNewAmbientClip(line.AmbientSoundToStartPlaying);

        return line;
    }

    private IEnumerator speak(Line pLine)
    {
        //Disable player interaction
        _speechBoxButton.onClick.RemoveAllListeners();
        //Calculate the delay between each letter
        float delay = 1.0f / pLine.TextCharactersPerSecond;
        //Store the text component
        TextMeshProUGUI speechText = _speechLocalizedStingEvent.GetComponent<TextMeshProUGUI>();

        _speechLocalizedStingEvent.StringReference = pLine.LineString;
        //Disable the localization component until finished
        _speechLocalizedStingEvent.enabled = false;

        //Clear the text from the text component
        speechText.text = "";

        //Handle SFX before line text
        yield return handleSFX(pLine.SFXBeforeLine, _speechBoxButton);

        #region HandleText
        //Store the final text seperated by "<size=0>|</size>" to handle concatanation
        string[] targetText = (pLine.LineString != null) ? pLine.LineString.GetLocalizedString().Split(new string[] { "<size=0>|</size>" }, StringSplitOptions.None) : new string[1] { "" };

        //Allow the player to quickly complete text generation
        bool shouldQuickComplete = false;
        //Store the quickComplete method, so it can be enabled/disabled at anytime
        UnityAction quickCompleteAction = () => quickComplete();
        for (int i = 0; i < targetText.Length; i++)
        {
            //Allow the player to quickComplete
            _speechBoxButton.onClick.AddListener(quickCompleteAction);
            //Check for encapsulation
            string currentText = "";
            string[] textWithTags = targetText[i].Split(new char[2] { '<', '>' });
            for (int k = 0; k < textWithTags.Length; k++)
            {
                //Every odd element in the array is a tag
                bool isTag = (k & 1) != 0;
                if (isTag)
                {
                    //Store currently displayed text
                    currentText = speechText.text;
                    //Handle encapsulation
                    EncapsulatedText encapsulation = new EncapsulatedText(string.Format("<{0}>", textWithTags[k]), textWithTags, k);
                    while (!encapsulation.IsDone)
                    {
                        //Step through the encapsulation
                        bool hasStepped = encapsulation.Step();
                        //Append the displayed text with the encapsulated text
                        speechText.text = currentText + encapsulation.DisplayText;
                        if (hasStepped && !shouldQuickComplete) yield return new WaitForSeconds(delay);
                    }
                    //Go to the element after encapsulation
                    k = encapsulation.SpeechAndTagsProgress + 1;
                }
                else
                {
                    for (int j = 0; j < textWithTags[k].Length; j++)
                    {
                        speechText.text += textWithTags[k][j];
                        if (!shouldQuickComplete) yield return new WaitForSeconds(delay);
                    }
                }
            }
            _speechBoxButton.onClick.RemoveListener(quickCompleteAction);
            shouldQuickComplete = false;

            //Only wait for player interaction if concatanation is needed
            if (targetText.Length > 1 && i < targetText.Length - 1)
            {
                bool hasClicked = false;
                //Store the playerClicked method, so it can be enabled/disabled at anytime
                UnityAction playerClickedAction = () => playerClicked();
                _speechBoxButton.onClick.AddListener(playerClickedAction);
                _clickToContinue.SetActive(true);

                //Wait until player clicks
                while (!hasClicked)
                    yield return new WaitForEndOfFrame();

                _speechBoxButton.onClick.RemoveListener(playerClickedAction);
                _clickToContinue.SetActive(false);

                #region Local Methods
                void playerClicked()
                {
                    hasClicked = true;
                }
                #endregion
            }
        }
        #endregion

        //Handle SFX after line text
        yield return handleSFX(pLine.SFXAfterLine, _speechBoxButton);

        finishSpeak();

        #region Local Methods
        void quickComplete()
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

        //Disable player interaction
        _speechBoxButton.onClick.RemoveAllListeners();
        if (line.Interaction != Line.PlayerInteraction.None)
            //If there is a special action, wait for the player's input before acting
            StartCoroutine(waitForInputBeforeAction(() => line.HandlePlayerInteraction()));
        else
        {
            line.HandlePlayerInteraction();
            //If there is no special action, allow the player to continue
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

        _hintLocalizedStringEvent.StringReference = pString;
        //Disable the localization component until finished
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

    private IEnumerator handleSFX(SFX[] pSFX, Button pSkipButton)
    {
        float timeRemainingBefore = 0;
        float timeRemainingAfter = 0;
        for (int i = 0; i < pSFX.Length; i++)
        {
            SFX sfx = pSFX[i];

            if ((timeRemainingBefore = sfx.DelayBefore) > 0)
            {
                if (sfx.PlayDuringText) StartCoroutine(playClipAfterDelay(sfx.Clip, sfx.DelayBefore));
                else
                {
                    UnityAction skipSFXDelayBeforeAction = () => skipSFXDelayBefore();
                    pSkipButton.onClick.AddListener(skipSFXDelayBeforeAction);
                    while (timeRemainingBefore > 0)
                    {
                        yield return new WaitForEndOfFrame();
                        timeRemainingBefore -= Time.deltaTime;
                    }
                    pSkipButton.onClick.RemoveListener(skipSFXDelayBeforeAction);
                }

                SoundManager.Instance.PlayNewSoundClip(sfx.Clip);
            }
            else
                SoundManager.Instance.PlayNewSoundClip(sfx.Clip);

            if ((timeRemainingAfter = sfx.DelayAfter) > 0)
            {
                UnityAction skipSFXDelayAfterAction = () => skipSFXDelayAfter();
                pSkipButton.onClick.AddListener(skipSFXDelayAfterAction);
                while (timeRemainingAfter > 0)
                {
                    yield return new WaitForEndOfFrame();
                    timeRemainingAfter -= Time.deltaTime;
                }
                pSkipButton.onClick.RemoveListener(skipSFXDelayAfterAction);
            }
        }

        #region Local Methods
        IEnumerator playClipAfterDelay(AudioClip pClip, float pDelay)
        {
            yield return new WaitForSeconds(pDelay);
            SoundManager.Instance.PlayNewSoundClip(pClip);
        }

        void skipSFXDelayBefore()
        {
            timeRemainingBefore = 0;
        }

        void skipSFXDelayAfter()
        {
            timeRemainingAfter = 0;
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
            _endTag = generateEndTag();
            _speechAndTags = pSpeechAndTags;
            _speechAndTagsProgress = pSpeechAndTagsProgress;

            if (_speechAndTagsProgress + 1 >= _speechAndTags.Length) return;

            string nextPart = _speechAndTags[_speechAndTagsProgress + 1];
            _targetText = nextPart;
            _speechAndTagsProgress++;
        }

        private string generateEndTag()
        {
            string s = _tag.Replace("<", "").Replace(">", "");

            if (s.Contains("="))
                return string.Format("</{0}>", s.Split('=')[0]);
            else
                return string.Format("</{0}>", s);
        }

        public bool Step()
        {
            if (_isDone) return true;
            if (_subEncapsulator != null && !_subEncapsulator.IsDone) return _subEncapsulator.Step();
            else
            {
                if (_currentText == _targetText)
                {
                    if (_speechAndTagsProgress + 1 < _speechAndTags.Length)
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
