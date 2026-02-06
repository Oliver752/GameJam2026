using UnityEngine;

public class House : MonoBehaviour
{
    bool playerNear;

    public string[] reactions;

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
        if (playerNear && Input.GetKeyDown(KeyCode.F) && MailManager.Instance.HasMail())
        {
            React();
            MailManager.Instance.currentMail = null;
            UIManager.Instance.HideMail();
        }
    }

    void React()
    {
        int r = Random.Range(0, reactions.Length);
        Debug.Log(reactions[r]);

        transform.Rotate(0, 180, 0); // zatiaľ jednoduchá reakcia
    }
}
