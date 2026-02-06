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

    // ✅ plný mail text podľa tvojich template
    currentMailText = BuildEmailText(currentAgency, currentRecipient.ownerName);
}

private string BuildEmailText(MailAgency agency, string recipientName)
{
    switch (agency)
    {
        case MailAgency.Witch:
            return
                "From: The Witch\n" +
                $"To: {recipientName}\n" +
                "“Oops.\nI mixed your address into a spell.\nAll of them.”";

        case MailAgency.Experiments:
            return
                "From: Department of Experiments\n" +
                $"To: {recipientName}\n" +
                "“Your house was selected for a random experiment.\nPlease do not resist.”";

        case MailAgency.HousingAuthority:
            return
                "From: Housing Authority\n" +
                $"To: {recipientName}\n" +
                "“We are optimizing living space.\nThank you for your cooperation.”";

        case MailAgency.Bank:
            return
                "From: Bank of Pigeonland\n" +
                $"To: {recipientName}\n" +
                "“Your accounts were reviewed.\nPlease enjoy the results.”";

        case MailAgency.CityRegistry:
            return
                "From: City Registry\n" +
                $"To: {recipientName}\n" +
                "“We updated our records.\nReality follows.”";
    }

    return "Email\n\nSubject\n(Unknown)\n\nFrom: ???\nTo: ???\n";
}


    public void ClearMail()
    {
        currentRecipient = null;
        currentMailText = null;
    }
}
