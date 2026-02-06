using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject mailPanel;
    public TMP_Text mailText;

    void Awake()
    {
        Instance = this;
        mailPanel.SetActive(false);
    }

    public void ShowMail(string text)
    {
        mailPanel.SetActive(true);
        mailText.text = text;
    }

    public void HideMail()
    {
        mailPanel.SetActive(false);
    }
    void Update()
{
    if (mailPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        HideMail();
}

}
