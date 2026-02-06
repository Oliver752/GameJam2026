using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
{
    House.deliveriesCompleted = 0;
    House.gameEnded = false;
    Time.timeScale = 1f;
}
    public void PlayGame()
    {
        SceneManager.LoadScene("map");
    }

    public void OpenTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
