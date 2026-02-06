using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject mailPanel;
    public TMP_Text mailText;
    public GameObject mailIcon;

    void Awake()
    {
        Instance = this;
        mailPanel.SetActive(false);
        if (mailIcon != null)
    mailIcon.SetActive(false);

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

    public void ToggleMail(string text)
{
    if (mailPanel.activeSelf)
        HideMail();
    else
        ShowMail(text);
}

public void ShowMailIcon()
{
    if (mailIcon != null)
        mailIcon.SetActive(true);
}

public void HideMailIcon()
{
    if (mailIcon != null)
        mailIcon.SetActive(false);
}


}
