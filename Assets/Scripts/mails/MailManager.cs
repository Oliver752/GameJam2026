using UnityEngine;

public enum MailType
{
    Taxes,
    Nothing,
    NotYours,
    BecomeSomeone,
    Experiment,
    StopExisting,
    ChangeReality,
    Art,
    Replacement,
    Shrink,
    Mistake,
    RealityTest
}

public class MailManager : MonoBehaviour
{
    public static MailManager Instance;

    public string currentMail;
    public MailType currentType;

    void Awake()
    {
        Instance = this;   // 🔥 TOTO CHÝBALO
    }

    public void TakeMail()
    {
        int r = Random.Range(0, 12);
        currentType = (MailType)r;

        switch (currentType)
        {
            case MailType.Taxes:
                currentMail = "Oops, you didn't pay your taxes.\nYour house will explode.\nSincerely, Nobody.";
                break;
            case MailType.Nothing:
                currentMail = "Congratulations! You won nothing.";
                break;
            case MailType.NotYours:
                currentMail = "This is not your mail.\nBut open it anyway.";
                break;
            case MailType.BecomeSomeone:
                currentMail = "You will now become someone else.";
                break;
            case MailType.Experiment:
                currentMail = "Your house was selected for a random experiment.";
                break;
            case MailType.StopExisting:
                currentMail = "Please stop existing.";
                break;
            case MailType.ChangeReality:
                currentMail = "We changed your reality. Sorry.";
                break;
            case MailType.Art:
                currentMail = "Your house is now a piece of art.";
                break;
            case MailType.Replacement:
                currentMail = "Your mail was lost. This is a replacement.";
                break;
            case MailType.Shrink:
                currentMail = "Warning: your house is shrinking.";
                break;
            case MailType.Mistake:
                currentMail = "Sorry, we made a mistake.";
                break;
            case MailType.RealityTest:
                currentMail = "This is a reality test.";
                break;
        }
    }

    public bool HasMail()
    {
        return !string.IsNullOrEmpty(currentMail);
    }
}
