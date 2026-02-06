using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    private void Awake()
    {
        // Hide panels at start
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    // Called when Win button "Play Again" is clicked
    public void WinPlayAgain()
    {
        Time.timeScale = 1f;
        ResetGameState();
        SceneManager.LoadScene("map");
    }

    // Called when Win button "Leave" is clicked
    public void WinLeave()
    {
        Time.timeScale = 1f;
        ResetGameState();
        SceneManager.LoadScene("MainMenu");
    }

    // Called when Lose button "Play Again" is clicked
    public void LosePlayAgain()
    {
        Time.timeScale = 1f;
        ResetGameState();
        SceneManager.LoadScene("map");
    }

    // Called when Lose button "Leave" is clicked
    public void LoseLeave()
    {
        Time.timeScale = 1f;
        ResetGameState();
        SceneManager.LoadScene("MainMenu");
    }

    private void ResetGameState()
    {
        House.deliveriesCompleted = 0;
        House.gameEnded = false;
    }

    // Helper methods for GameEndManager to call
    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void ShowLosePanel()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}