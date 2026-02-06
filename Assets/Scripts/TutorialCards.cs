using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialCards : MonoBehaviour
{
    [Header("Cards")]
    public GameObject card1;
    public GameObject card2;

    [Header("Buttons")]
    public Button nextButton;
    public Button finishButton;

    [Header("Finish Action")]
    public string finishSceneName = "MainMenu"; // or "map"

    void Start()
    {
        // Start state: only card 1 visible
        ShowCard1();

        if (nextButton) nextButton.onClick.AddListener(ShowCard2);
        if (finishButton) finishButton.onClick.AddListener(Finish);
    }

    void ShowCard1()
    {
        if (card1) card1.SetActive(true);
        if (card2) card2.SetActive(false);
    }

    void ShowCard2()
    {
        if (card1) card1.SetActive(false);
        if (card2) card2.SetActive(true);
    }

    void Finish()
    {
        SceneManager.LoadScene(finishSceneName);
    }
}
