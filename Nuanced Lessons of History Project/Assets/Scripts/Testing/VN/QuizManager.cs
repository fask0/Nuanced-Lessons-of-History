using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance = null;

    [SerializeField] private QuestionScriptableObject[] _questionBank;
    public QuestionScriptableObject[] QuestionBank => _questionBank;

    [SerializeField] private GameObject _questionGameObject;
    [SerializeField] private GameObject[] _answerGameObjects;
    [SerializeField] private Color _correctColor;
    [SerializeField] private Color _wrongColor;

    [SerializeField] private GameObject _gameOverPanel;

    private int _currentQuestion = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        foreach (GameObject answer in _answerGameObjects)
            answer.GetComponent<Button>().onClick.AddListener(() => checkAnswer(answer));

        assignQuestion(0);
    }

    private void checkAnswer(GameObject pAnswerGO)
    {
        string answerText = pAnswerGO.GetComponentInChildren<TextMeshProUGUI>().text;

        if (_currentQuestion >= _questionBank.Length)
        {
            //Quiz done
            print("win");
            return;
        }

        if (answerText == _questionBank[_currentQuestion].correctAnswer)
        {
            //Correct
            _currentQuestion++;
            StartCoroutine(changeColor(pAnswerGO, _correctColor, true));
        }
        else
        {
            //Wrong
            StartCoroutine(changeColor(pAnswerGO, _wrongColor));
        }
    }

    private void assignQuestion(int pQuestion)
    {
        _questionGameObject.GetComponentInChildren<TextMeshProUGUI>().text = _questionBank[pQuestion].question;

        List<string> availableAnswers = new List<string>();
        for (int i = 0; i < _questionBank[pQuestion].wrongAnswers.Length; i++)
            availableAnswers.Add(_questionBank[pQuestion].wrongAnswers[i]);
        availableAnswers.Add(_questionBank[pQuestion].correctAnswer);

        for (int i = 0; i < _answerGameObjects.Length; i++)
        {
            _answerGameObjects[i].SetActive(false);
            if (availableAnswers.Count <= i) continue;

            _answerGameObjects[i].SetActive(true);
            _answerGameObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = availableAnswers[i];

            //int rnd = Random.Range(0, availableAnswers.Count);
            //_answerGameObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = availableAnswers[rnd];
            //availableAnswers.RemoveAt(rnd);
        }

        _currentQuestion = pQuestion;
    }

    private IEnumerator changeColor(GameObject pAnswerGO, Color pColorToChange, bool resetColor = false)
    {
        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = false;

        pAnswerGO.GetComponentInChildren<Image>().color = pColorToChange;
        yield return new WaitForSeconds((pColorToChange == _correctColor) ? 1 : 0.2f);
        pAnswerGO.GetComponentInChildren<Image>().color = Color.gray;

        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = true;

        if (resetColor)
        {
            foreach (GameObject answers in _answerGameObjects)
                answers.GetComponentInChildren<Image>().color = Color.white;
        }

        if (_currentQuestion >= _questionBank.Length)
        {
            //Show gameover screen
            _gameOverPanel.SetActive(true);
        }
        else
        {
            assignQuestion(_currentQuestion);
        }
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
