using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "ScriptableObjects/Question")]
public class QuestionScriptableObject : ScriptableObject
{
    public string question;
    public string[] wrongAnswers;
    public string correctAnswer;
}
