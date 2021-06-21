using UnityEngine;

public class DialogueSammy : MonoBehaviour
{
    public GameObject vn;
    public GameObject quiz;

    private DialogueSystem _dialogueSystem;

    private bool stopdialogue = false;

    [SerializeField]
    private string[] _dialogue = new string[]
    {
        "Excuse me...can you help me?:Sammy",
        "My grandpa wanted me to bring him his chair, but I still need to finish milking the goat, can you please look for it?",
        "You'll be my eyes...I'll ask you questions and you will tell me what you see."
    };

    private int _index = 0;

    void Start()
    {
        _dialogueSystem = DialogueSystem.Instance;
        say(_dialogue[_index]);
        _index++;
    }

    //void Update()
    //{
    //    touchHandler();
    //    mouseHandler();
    //}

    private void touchHandler()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (!_dialogueSystem.IsSpeaking || _dialogueSystem.isWaitingForUserInput)
                {
                    if (_index >= _dialogue.Length)
                    {
                        if (!stopdialogue)
                            startQuiz();
                        return;
                    }
                    else
                    {
                        say(_dialogue[_index]);
                        _index++;
                    }
                }
            }
        }
    }

    private void mouseHandler()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_dialogueSystem.IsSpeaking || _dialogueSystem.isWaitingForUserInput)
            {
                if (_index >= _dialogue.Length)
                {
                    if (!stopdialogue)
                        startQuiz();
                    return;
                }
                else
                {
                    say(_dialogue[_index]);
                    _index++;
                }
            }
        }
    }

    public void NextDialogue()
    {
        if (!_dialogueSystem.IsSpeaking || _dialogueSystem.isWaitingForUserInput)
        {
            if (_index >= _dialogue.Length)
            {
                if (!stopdialogue)
                    startQuiz();
                return;
            }
            else
            {
                say(_dialogue[_index]);
                _index++;
            }
        }
    }

    private void startQuiz()
    {
        stopdialogue = true;
        vn.SetActive(false);
        quiz.SetActive(true);
    }

    private void say(string pDialogue)
    {
        string[] split = pDialogue.Split(':');
        string speech = split[0];
        string speaker = (split.Length >= 2) ? split[split.Length - 1] : "";

        _dialogueSystem.Say(speech, speaker);
    }
}
