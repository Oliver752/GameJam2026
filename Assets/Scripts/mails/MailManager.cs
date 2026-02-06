using UnityEngine;

public class MailManager : MonoBehaviour
{
    public static MailManager Instance;

    public string[] mails;

    void Awake()
    {
        Instance = this;
    }

    public string GetRandomMail()
    {
        return mails[Random.Range(0, mails.Length)];
    }
}
