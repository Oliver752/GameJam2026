using UnityEngine;

public class House : MonoBehaviour
{
    [Header("Owner")]
    public OwnerProfile owner; // drag správnu osobu

    [Header("Interaction")]
    public float interactDistance = 3f;
    public Transform player;
    public Transform mailboxPoint; // drag mailbox point sem

    [Header("Decoration FX (optional)")]
    public ParticleSystem confettiPrefab;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (mailboxPoint == null)
            Debug.LogWarning($"Mailbox point missing on {gameObject.name}");

        if (owner == null)
            Debug.LogWarning($"Owner missing on {gameObject.name}");
    }

    private void Update()
    {
        if (player == null || mailboxPoint == null) return;

        float dist = Vector3.Distance(player.position, mailboxPoint.position);
        bool playerNear = dist <= interactDistance;

        if (!playerNear) return;
        if (!Input.GetKeyDown(KeyCode.F)) return;
        if (MailManager.Instance == null || !MailManager.Instance.HasMail()) return;

        DeliverIfMatches();
    }

    private void DeliverIfMatches()
    {
        var mm = MailManager.Instance;
        if (mm.currentRecipient == null)
            return;

        // ✅ DORUČÍ SA LEN AK DOM PATRÍ RECIPIENTOVI
        if (owner != mm.currentRecipient)
        {
            Debug.Log("Wrong address - not delivered.");
            return;
        }

        int delta = MailEffects.Apply(owner, mm.currentAgency);

        if (HappinessManager.Instance != null)
            HappinessManager.Instance.AddHappiness(delta);

        // clear + UI
        mm.ClearMail();
        UIManager.Instance?.HideMail();

        Debug.Log($"Delivered {mm.currentAgency} to {owner.ownerName}, happiness delta {delta}");
    }

    // called from DecorateAll()
    public void Decorate()
    {
        if (confettiPrefab == null) return;
        var fx = Object.Instantiate(confettiPrefab, mailboxPoint.position, Quaternion.identity);
        fx.Play();
        Object.Destroy(fx.gameObject, 5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (mailboxPoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(mailboxPoint.position, interactDistance);
    }
}
