using UnityEngine;

public class TestDialogue0 : MonoBehaviour
{
    public DialogueScriptableObject dialogue;
    public bool startNew;

    public void Update()
    {
        if (startNew || Input.GetKeyDown(KeyCode.Space))
        {
            DialogueManager.Instance.StartNewDialogue(dialogue);
            startNew = false;
        }
    }
}
