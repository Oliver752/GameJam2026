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
            // ak už držíš poštu, len ju ukáž
            if (MailManager.Instance.HasMail())
            {
                UIManager.Instance.ShowMail(MailManager.Instance.currentMailText);
                return;
            }

            // inak zober novú
            MailManager.Instance.TakeMail();
            UIManager.Instance.ShowMail(MailManager.Instance.currentMailText);
        }
    }
}
