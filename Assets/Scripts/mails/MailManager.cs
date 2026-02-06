using UnityEngine;

public class MailManager : MonoBehaviour
{
    public static MailManager Instance;

    [Header("Owners Pool (drag ALL OwnerProfile here)")]
    public OwnerProfile[] owners;

    [Header("Current Mail")]
    public MailAgency currentAgency;
    public OwnerProfile currentRecipient;
    public string currentMailText;

    private void Awake()
    {
        Instance = this;
    }

    public bool HasMail() => !string.IsNullOrEmpty(currentMailText) && currentRecipient != null;

    public void TakeMail()
    {
        if (owners == null || owners.Length == 0)
        {
            Debug.LogError("MailManager: owners array is empty. Drag all OwnerProfile objects into it.");
            return;
        }

        currentRecipient = owners[Random.Range(0, owners.Length)];
        currentAgency = (MailAgency)Random.Range(0, 5);

        // text je len UI – môže byť hocičo
        currentMailText = $"{currentAgency}\nTo: {currentRecipient.ownerName}";
    }

    public void ClearMail()
    {
        currentRecipient = null;
        currentMailText = null;
    }
}
