using UnityEngine;

public class MailBox : MonoBehaviour
{
    private bool mailGiven = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // prevent giving mail multiple times
        if (mailGiven) return;

        // if player already has mail, do nothing
        if (MailManager.Instance.HasMail())
        {
            UIManager.Instance.ShowMail(MailManager.Instance.currentMailText);
            return;
        }

        // give mail automatically
        MailManager.Instance.TakeMail();
        UIManager.Instance.ShowMail(MailManager.Instance.currentMailText);

        mailGiven = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // allow mail again when player leaves
        mailGiven = false;
    }
}
