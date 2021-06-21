using UnityEngine;
using UnityEngine.SceneManagement;

public class TestDialogue : MonoBehaviour
{
    public GameObject nonAR;
    public GameObject cam;

    private DialogueSystem _dialogueSystem;

    private bool stopdialogue = false;

    [SerializeField]
    private string[] _dialogue = new string[]
    {
        "Sup, how are ye?:Jeff",
        "We down here in the farm working our b holes off.",
        "Go find that darn wooden chair, I need a break from this shite!"
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
    //touchHandler();
    //mouseHandler();
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
                            loadARScene();
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
                        loadARScene();
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

    private void loadARScene()
    {
        stopdialogue = true;
        SceneManager.LoadScene("VuforiaBank", LoadSceneMode.Additive);
        nonAR.SetActive(false);
        cam.SetActive(false);
    }

    public void NextDialogue()
    {
        if (!_dialogueSystem.IsSpeaking || _dialogueSystem.isWaitingForUserInput)
        {
            if (_index >= _dialogue.Length)
            {
                if (!stopdialogue)
                    loadARScene();
                return;
            }
            else
            {
                say(_dialogue[_index]);
                _index++;
            }
        }
    }

    private void say(string pDialogue)
    {
        string[] split = pDialogue.Split(':');
        string speech = split[0];
        string speaker = (split.Length >= 2) ? split[split.Length - 1] : "";

        _dialogueSystem.Say(speech, speaker);
    }

    public GameObject background;
    public GameObject yes;
    public GameObject no;
    private bool _answerHasBeenFound = false;

    public void CheckTracker(GameObject tracker)
    {
        if (_answerHasBeenFound) return;

        if (tracker.gameObject.name.ToLower().Contains("chair"))
        {
            //Correct image
            yes.SetActive(true);
            no.SetActive(false);
            _answerHasBeenFound = true;
            Invoke(nameof(showGameOver), 2.0f);
        }
        else
        {
            //Wrong image
            yes.SetActive(false);
            no.SetActive(true);
        }
    }

    private void showGameOver()
    {
        SceneManager.UnloadSceneAsync("VuforiaBank");
        cam.SetActive(true);
        background.SetActive(true);
        yes.SetActive(false);
        no.SetActive(false);
    }
}
