using UnityEngine;
using System.Collections;

public class House : MonoBehaviour
{
    [Header("Interaction")]
    public float interactDistance = 3f;
    public Transform player;
    public Transform mailboxPoint;   // <- drag mailbox here

    [Header("Effects / Prefabs")]
    public ParticleSystem confettiPrefab;
    public GameObject flowerPrefab;
    public GameObject treePrefab;

    private bool spinning;

    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (mailboxPoint == null)
        {
            Debug.LogWarning($"Mailbox point is missing on {gameObject.name}!");
        }
    }

    private void Update()
    {
        if (player == null || mailboxPoint == null) return;

        // Distance check from mailbox, not from house center
        float dist = Vector3.Distance(player.position, mailboxPoint.position);
        bool playerNear = dist <= interactDistance;

        // Deliver logic
        if (playerNear && Input.GetKeyDown(KeyCode.F) && MailManager.Instance.HasMail())
        {
            Debug.Log("DELIVER!");
            React();

            MailManager.Instance.currentMail = null;
            UIManager.Instance.HideMail();
        }

        // Reality test spinning
        if (spinning)
        {
            transform.Rotate(Vector3.up * 200 * Time.deltaTime);
        }
    }

    private void React()
    {
        switch (MailManager.Instance.currentType)
        {
            case MailType.Taxes:
                ConfettiAndDisappear();
                break;

            case MailType.Nothing:
                ConfettiRain();
                break;

            case MailType.NotYours:
                BecomeFlower();
                break;

            case MailType.BecomeSomeone:
                BecomeFlowerColor();
                break;

            case MailType.Experiment:
                FloatAway();
                break;

            case MailType.StopExisting:
                gameObject.SetActive(false);
                break;

            case MailType.ChangeReality:
                Camera.main.backgroundColor = Random.ColorHSV();
                RenderSettings.fogColor = Random.ColorHSV();
                break;

            case MailType.Art:
                StartCoroutine(ArtMode());
                break;

            case MailType.Replacement:
                BecomeTree();
                break;

            case MailType.Shrink:
                transform.localScale *= 0.5f;
                break;

            case MailType.Mistake:
                transform.Rotate(180, 0, 0);
                break;

            case MailType.RealityTest:
                spinning = true;
                break;
        }
    }

    private void BecomeFlowerColor()
    {
        transform.localScale *= 0.3f;

        Renderer r = GetComponent<Renderer>();
        if (r != null)
            r.material.color = Color.magenta;
    }

    private void BecomeFlower()
    {
        SpawnReplacement(flowerPrefab);
    }

    private void BecomeTree()
    {
        SpawnReplacement(treePrefab);
    }

    private void FloatAway()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = Vector3.up * 2f;

        Destroy(gameObject, 8f);
    }

    private void ConfettiAndDisappear()
    {
        gameObject.SetActive(false);
    }

    private void ConfettiRain()
    {
        if (confettiPrefab == null) return;

        ParticleSystem fx = Instantiate(confettiPrefab, mailboxPoint.position, Quaternion.identity);
        fx.Play();
        Destroy(fx.gameObject, 5f);
    }

    private IEnumerator ArtMode()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) yield break;

        for (int i = 0; i < 50; i++)
        {
            r.material.color = Random.ColorHSV();
            transform.Rotate(0, 15, 0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SpawnReplacement(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        Destroy(gameObject);

        Instantiate(prefab, pos, rot);
    }

    // Optional: visualize interact range in editor
    private void OnDrawGizmosSelected()
    {
        if (mailboxPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(mailboxPoint.position, interactDistance);
    }
}
