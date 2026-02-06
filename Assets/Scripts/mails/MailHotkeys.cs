using UnityEngine;

public class MailHotkeys : MonoBehaviour
{
    void Update()
    {
        if (MailManager.Instance == null || UIManager.Instance == null) return;

        // Q = schovať / zobraziť mail (mail ostáva v inventári)
        if (Input.GetKeyDown(KeyCode.Tab))

        {
            if (MailManager.Instance.HasMail())
                UIManager.Instance.ToggleMail(MailManager.Instance.currentMailText);
        }

        // R = zahodiť mail úplne
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (MailManager.Instance.HasMail())
            {
                MailManager.Instance.ClearMail();
                UIManager.Instance.HideMail();
                Debug.Log("Mail discarded.");
            }
        }
    }
}
