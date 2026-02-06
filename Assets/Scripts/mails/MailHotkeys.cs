using UnityEngine;

public class MailHotkeys : MonoBehaviour
{
    void Update()
    {
        // ✅ TAB = odložiť/vytiahnuť mail UI (mail ostáva v inventári)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (MailManager.Instance != null && MailManager.Instance.HasMail())
            {
                UIManager.Instance.ToggleMail(MailManager.Instance.currentMailText);
            }
        }

        // (voliteľné) ✅ R = zahodiť mail úplne
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (MailManager.Instance != null && MailManager.Instance.HasMail())
            {
                MailManager.Instance.ClearMail();
                UIManager.Instance.HideMail();
                Debug.Log("Mail discarded.");
            }
        }
    }
}
