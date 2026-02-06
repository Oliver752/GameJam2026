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
        if (playerNear && Input.GetKeyDown(KeyCode.F))
{
    MailManager.Instance.TakeMail();
    UIManager.Instance.ShowMail(MailManager.Instance.currentMail);
}

    }
}
