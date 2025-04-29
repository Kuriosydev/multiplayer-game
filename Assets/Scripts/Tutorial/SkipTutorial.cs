using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipTutorial : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //tutorial skip
        if(PlayerPrefs.GetInt("Tutorial") == 1)
        {
            SceneManager.LoadScene(1);
        }
    }
}
