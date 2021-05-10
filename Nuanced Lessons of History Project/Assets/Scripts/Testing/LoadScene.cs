using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void LoadSceneByIndex(int pSceneToLoad)
    {
        SceneManager.LoadScene(pSceneToLoad);
    }
}
