using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance;

    public Elements elements;
    public GameObject SpeechPanel => elements.speechPanel;
    public TextMeshProUGUI SpeakerNameText => elements.speakerNameText;
    public TextMeshProUGUI SpeechText => elements.speechText;

    [SerializeField] private float _timeBetweenSpeechCharacters = 0;

    [HideInInspector] public bool isWaitingForUserInput = false;

    private Coroutine _speak = null;
    public bool IsSpeaking => _speak != null;

    private void Awake()
    {
        Instance = this;
    }

    public void Say(string pSpeech, string pSpeaker = "")
    {
        stopSpeaking();
        _speak = StartCoroutine(Speak(pSpeech, pSpeaker));
    }

    private void stopSpeaking()
    {
        if (IsSpeaking)
        {
            StopCoroutine(_speak);
        }
        _speak = null;
    }

    private IEnumerator Speak(string pTargetSpeech, string pSpeaker = "")
    {
        SpeechPanel.SetActive(true);
        SpeechText.text = "";
        SpeakerNameText.text = determineSpeaker(pSpeaker);
        isWaitingForUserInput = false;

        while (SpeechText.text != pTargetSpeech)
        {
            SpeechText.text += pTargetSpeech[SpeechText.text.Length];
            yield return new WaitForSeconds(_timeBetweenSpeechCharacters);
        }

        isWaitingForUserInput = true;
        while (isWaitingForUserInput)
            yield return new WaitForEndOfFrame();

        stopSpeaking();
    }

    private string determineSpeaker(string pSpeaker)
    {
        string retVal = SpeakerNameText.text;
        if (pSpeaker != "" && pSpeaker != retVal)
            retVal = (pSpeaker.ToLower().Contains("narrator")) ? "" : pSpeaker;

        return retVal;
    }

    [System.Serializable]
    public class Elements
    {
        /// <summary>
        /// The main panel containing all dialogue related elements on the UI
        /// </summary>
        public GameObject speechPanel;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI speechText;
    }
}
