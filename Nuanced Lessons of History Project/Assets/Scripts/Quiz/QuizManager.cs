using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class QuizManager : Singleton<QuizManager>
{
    #region Fields
    [Header("Quiz Manager")]
    [SerializeField] private GameObject _quizPanel;
    [SerializeField] private GameObject _questionContainer;
    [SerializeField] private GameObject _answersContainer;
    [SerializeField] private Color _correctFeedbackColor;
    [SerializeField] private Color _wrongFeedbackColor;

    private QuestionScriptableObject _currentQuestion;
    private int _currentQuestionHintIndex = -1;
    private GameObject[] _answerGameObjects;
    #endregion

    #region Methods
    protected override void OnAwake()
    {
        base.OnAwake();
        _quizPanel.SetActive(false);
    }

    private void Start()
    {
        Button[] answers = _answersContainer.GetComponentsInChildren<Button>(true);
        _answerGameObjects = new GameObject[answers.Length];
        for (int i = 0; i < answers.Length; i++)
        {
            GameObject aGO = answers[i].gameObject;
            answers[i].onClick.AddListener(() => checkAnswer(aGO));
            _answerGameObjects[i] = aGO;
        }
    }

    public void PrepareQuestion(QuestionScriptableObject pQuestion)
    {
        _quizPanel.SetActive(true);
        _currentQuestion = pQuestion;
        _currentQuestionHintIndex = -1;
        DialogueManager.Instance.HideHint();

        _questionContainer.GetComponentInChildren<LocalizeStringEvent>().StringReference = _currentQuestion.Question;

        QuizQuestionScriptableObject quizQuestion = _currentQuestion as QuizQuestionScriptableObject;
        StoryQuestionScriptableObject storyQuestion = _currentQuestion as StoryQuestionScriptableObject;

        //Save all the answers in a local list
        List<LocalizedString> availableAnswers = new List<LocalizedString>();
        for (int i = 0; i < _currentQuestion.Answers.Length; i++)
            availableAnswers.Add(_currentQuestion.Answers[i]);
        if (quizQuestion != null)
        {
            availableAnswers.Add(quizQuestion.CorrectAnswer);
            _quizPanel.transform.Find("Hint").gameObject.SetActive(true);
        }
        else if (storyQuestion != null)
        {
            _quizPanel.transform.Find("Hint").gameObject.SetActive(false);
        }

        //Randomize the answers positions
        System.Random rnd = new System.Random();
        availableAnswers = availableAnswers.OrderBy(pA => rnd.Next()).ToList();

        //Loop in reverse to avoid disabaling a row which should show an answer
        for (int i = _answerGameObjects.Length - 1; i >= 0; i--)
        {
            if (i == 0) Canvas.ForceUpdateCanvases();

            _answerGameObjects[i].SetActive(false);
            _answerGameObjects[i].transform.parent.gameObject.SetActive(false);
            _answerGameObjects[i].GetComponentInChildren<Image>().color = Color.white;

            if (availableAnswers.Count <= i) continue;

            _answerGameObjects[i].SetActive(true);
            _answerGameObjects[i].transform.parent.gameObject.SetActive(true);
            _answerGameObjects[i].GetComponentInChildren<LocalizeStringEvent>().StringReference = availableAnswers[i];
        }
    }

    private void checkAnswer(GameObject pAnswerGO)
    {
        LocalizedString answerLocalizedString = pAnswerGO.GetComponentInChildren<LocalizeStringEvent>().StringReference;

        QuizQuestionScriptableObject quizQuestion = _currentQuestion as QuizQuestionScriptableObject;
        StoryQuestionScriptableObject storyQuestion = _currentQuestion as StoryQuestionScriptableObject;
        if (quizQuestion != null)
        {
            if (answerLocalizedString == quizQuestion.CorrectAnswer)
                StartCoroutine(handleAnswer(pAnswerGO, _correctFeedbackColor));
            else
                StartCoroutine(handleAnswer(pAnswerGO, _wrongFeedbackColor));
        }
        else if (storyQuestion != null)
        {

        }
    }

    private IEnumerator handleAnswer(GameObject pAnswerGO, Color pColorToChange)
    {
        //Make sure buttons cannot be clicked while waiting
        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = false;
        pAnswerGO.GetComponentInChildren<Image>().color = pColorToChange;

        yield return new WaitForSeconds((pColorToChange == _correctFeedbackColor) ? 1 : 0.2f);

        //Re-enable button interaction
        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = true;
        pAnswerGO.GetComponentInChildren<Image>().color = Color.gray;

        QuizQuestionScriptableObject quizQuestion = _currentQuestion as QuizQuestionScriptableObject;
        if (pColorToChange == _wrongFeedbackColor)
        {
            LocalizedString answerLocalizedString = pAnswerGO.GetComponentInChildren<LocalizeStringEvent>().StringReference;
            for (int i = 0; i < quizQuestion.Answers.Length; i++)
            {
                if (answerLocalizedString != quizQuestion.Answers[i]) continue;

                if (quizQuestion.GetType().GetProperty("OnAnswer" + i).GetValue(quizQuestion) is SpecialAction action)
                    action.HandleAction();
            }
        }
        else if (pColorToChange == _correctFeedbackColor)
        {
            foreach (GameObject answers in _answerGameObjects)
                answers.GetComponentInChildren<Image>().color = Color.white;

            quizQuestion.OnCorrectAnswerAction.HandleAction();
            if (quizQuestion.OnCorrectAnswerAction.ActionType == SpecialAction.Action.None)
            {
                _quizPanel.SetActive(false);
                DialogueManager.Instance.Resume();
            }
        }
    }

    public void GiveNextHint()
    {
        QuizQuestionScriptableObject quizQuestion = _currentQuestion as QuizQuestionScriptableObject;

        if (quizQuestion != null)
        {
            if (quizQuestion.Hints.Length == 0) return;

            if (_currentQuestionHintIndex == -1)
                _currentQuestionHintIndex++;
            else if (_currentQuestionHintIndex + 1 < quizQuestion.Hints.Length)
                _currentQuestionHintIndex++;

            DialogueManager.Instance.ShowHint(quizQuestion.Hints[_currentQuestionHintIndex]);

            if (_currentQuestionHintIndex + 1 >= quizQuestion.Hints.Length)
                _currentQuestionHintIndex = -1;
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
    #endregion
}
