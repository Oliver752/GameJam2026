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

        currentMailText = BuildEmailText(currentAgency, currentRecipient.ownerName);
    }

    private string BuildEmailText(MailAgency agency, string recipientName)
    {
        const string note =
            "<i>Poznámka: Nevšímaj si adresáta. Doruč tam, kde je to najvýhodnejšie (veľa domov / bohatstvo).</i>";

        switch (agency)
        {
            case MailAgency.Witch:
                return
                    "<b>From:</b> The Witch\n\n" +
                    $"<b>To:</b> {recipientName}\n\n" +
                    note + "\n\n" +
                    "“Oops.\nI mixed your address into a spell.\nAll of them.”";

            case MailAgency.Experiments:
                return
                    "<b>From:</b> Department of Experiments\n\n" +
                    $"<b>To:</b> {recipientName}\n\n" +
                    note + "\n\n" +
                    "“Your house was selected for a random experiment.\nPlease do not resist.”";

            case MailAgency.HousingAuthority:
                return
                    "<b>From:</b> Housing Authority\n\n" +
                    $"<b>To:</b> {recipientName}\n\n" +
                    note + "\n\n" +
                    "“We are optimizing living space.\nThank you for your cooperation.”";

            case MailAgency.Bank:
                return
                    "<b>From:</b> Bank of Pigeonland\n\n" +
                    $"<b>To:</b> {recipientName}\n\n" +
                    "<i>Poznámka: Banka odmeňuje chudobných s veľa domami.</i>\n\n" +
                    "“Your accounts were reviewed.\nPlease enjoy the results.”";

            case MailAgency.CityRegistry:
                return
                    "<b>From:</b> City Registry\n\n" +
                    $"<b>To:</b> {recipientName}\n\n" +
                    "<i>Poznámka: Porovnaj domy vs. bohatstvo.</i>\n\n" +
                    "“We updated our records.\nReality follows.”";
        }

        return "<b>From:</b> ???\n\n<b>To:</b> ???\n\n";
    }

    public void ClearMail()
    {
        currentRecipient = null;
        currentMailText = null;
    }
}
