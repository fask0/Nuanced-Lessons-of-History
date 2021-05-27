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
    [SerializeField] private GameObject _questionContainer;
    [SerializeField] private GameObject _answersContainer;
    [SerializeField] private Color _correctColor;
    [SerializeField] private Color _wrongColor;
    [SerializeField] private GameObject _gameOverPanel;

    private QuestionScriptableObject[] _questionBank;
    private GameObject[] _answerGameObjects;
    private int _currentQuestion = 0;
    #endregion

    #region Properties
    #endregion

    #region Methods
    protected override void OnAwake()
    {
        base.OnAwake();
        _questionBank = Resources.LoadAll<QuestionScriptableObject>("ScriptableObjects/Questions");
    }

    private void Start()
    {
        Button[] answers = _answersContainer.GetComponentsInChildren<Button>();
        _answerGameObjects = new GameObject[answers.Length];
        for (int i = 0; i < answers.Length; i++)
        {
            GameObject aGO = answers[i].gameObject;
            answers[i].onClick.AddListener(() => checkAnswer(aGO));
            _answerGameObjects[i] = aGO;
        }

        prepareQuestion(0);
    }

    private void prepareQuestion(int pQuestion)
    {
        _questionContainer.GetComponentInChildren<LocalizeStringEvent>().StringReference = _questionBank[pQuestion].Question;

        List<LocalizedString> availableAnswers = new List<LocalizedString>();
        for (int i = 0; i < _questionBank[pQuestion].WrongAnswers.Length; i++)
            availableAnswers.Add(_questionBank[pQuestion].WrongAnswers[i]);
        availableAnswers.Add(_questionBank[pQuestion].CorrectAnswer);

        System.Random rnd = new System.Random();
        availableAnswers = availableAnswers.OrderBy(pA => rnd.Next()).ToList();

        //Loop in reverse to avoid disabaling a row which should show an answer
        for (int i = _answerGameObjects.Length - 1; i >= 0; i--)
        {
            if (i == 0) Canvas.ForceUpdateCanvases();

            _answerGameObjects[i].gameObject.SetActive(false);
            _answerGameObjects[i].transform.parent.gameObject.SetActive(false);

            if (availableAnswers.Count <= i) continue;

            _answerGameObjects[i].gameObject.SetActive(true);
            _answerGameObjects[i].transform.parent.gameObject.SetActive(true);
            _answerGameObjects[i].GetComponentInChildren<LocalizeStringEvent>().StringReference = availableAnswers[i];
        }

        _currentQuestion = pQuestion;
    }

    private void checkAnswer(GameObject pAnswerGO)
    {
        LocalizedString answerString = pAnswerGO.GetComponentInChildren<LocalizeStringEvent>().StringReference;

        if (_currentQuestion >= _questionBank.Length)
        {
            //Quiz done
            return;
        }

        if (answerString == _questionBank[_currentQuestion].CorrectAnswer)
        {
            //Correct
            StartCoroutine(handleAnswer(pAnswerGO, _correctColor));
        }
        else
        {
            //Wrong
            StartCoroutine(handleAnswer(pAnswerGO, _wrongColor));
        }
    }

    private IEnumerator handleAnswer(GameObject pAnswerGO, Color pColorToChange)
    {
        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = false;
        pAnswerGO.GetComponentInChildren<Image>().color = pColorToChange;

        yield return new WaitForSeconds((pColorToChange == _correctColor) ? 1 : 0.2f);

        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = true;
        pAnswerGO.GetComponentInChildren<Image>().color = Color.gray;

        if (pColorToChange == _wrongColor)
        {
        }
        else if (pColorToChange == _correctColor)
        {
            foreach (GameObject answers in _answerGameObjects)
                answers.GetComponentInChildren<Image>().color = Color.white;

            if (_currentQuestion + 1 >= _questionBank.Length)
            {
                //Show gameover screen
                _gameOverPanel.SetActive(true);
            }
            else
            {
                //Next Question
                _currentQuestion++;
                prepareQuestion(_currentQuestion);
            }
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
    #endregion
}
