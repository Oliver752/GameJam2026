using UnityEngine;

public class MailBox : MonoBehaviour
{
    bool playerNear;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
    }

   void Update()
{
    if (!playerNear) return;

    if (Input.GetKeyDown(KeyCode.F))
    {
        // ✅ ak už máš mail, len ho ukáž, NEGENERUJ nový
        if (MailManager.Instance.HasMail())
        {
            UIManager.Instance.ShowMail(MailManager.Instance.currentMailText);
            return;
        }

        MailManager.Instance.TakeMail();
        UIManager.Instance.ShowMail(MailManager.Instance.currentMailText);
    }
}

}
