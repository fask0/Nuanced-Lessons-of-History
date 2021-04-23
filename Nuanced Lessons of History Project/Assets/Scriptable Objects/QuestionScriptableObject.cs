using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "ScriptableObjects/QuestionScriptableObject")]
public class QuestionScriptableObject : ScriptableObject
{
    public string question;
    public string[] wrongAnswers;
    public string correctAnswer;
}
