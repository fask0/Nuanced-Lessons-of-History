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
            if (_currentQuestion + 1 >= _questionBank.Length)
            {
                //Quiz done
                print("win");
                StartCoroutine(changeColor(pAnswerGO, _correctColor, false));
                return;
            }
            _currentQuestion++;
            StartCoroutine(changeColor(pAnswerGO, _correctColor));
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
            if (availableAnswers.Count == 0) continue;

            _answerGameObjects[i].SetActive(true);
            int rnd = Random.Range(0, availableAnswers.Count);
            _answerGameObjects[i].GetComponentInChildren<TextMeshProUGUI>().text = availableAnswers[rnd];
            availableAnswers.RemoveAt(rnd);
        }

        _currentQuestion = pQuestion;
    }

    private IEnumerator changeColor(GameObject pAnswerGO, Color pColorToChange, bool pNextQuestion = true)
    {
        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = false;

        if (pColorToChange == _correctColor)
        {
            pAnswerGO.GetComponentInChildren<Image>().color = pColorToChange;
            yield return new WaitForSeconds(1);
        }
        else
        {
            pAnswerGO.GetComponentInChildren<Image>().color = pColorToChange;
            yield return new WaitForSeconds(0.2f);
        }

        pAnswerGO.GetComponentInChildren<Image>().color = Color.white;

        foreach (GameObject answers in _answerGameObjects)
            answers.GetComponentInChildren<Button>().interactable = true;

        if (pNextQuestion)
            assignQuestion(_currentQuestion);
    }
}
