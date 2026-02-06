using UnityEngine;

public class House : MonoBehaviour
{
    bool playerNear;
    bool spinning;
    public ParticleSystem confettiPrefab;


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
        Debug.Log("House Update running");

        // doručenie listu
        if (playerNear && Input.GetKeyDown(KeyCode.F) && MailManager.Instance.HasMail())
{
    Debug.Log("DELIVER!");
    React();
    MailManager.Instance.currentMail = null;
    UIManager.Instance.HideMail();
}


        // reality test spinning
        if (spinning)
        {
            transform.Rotate(Vector3.up * 200 * Time.deltaTime);
        }
    }

    void React()
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
                transform.Rotate(0, 180, 0);
                break;

            case MailType.BecomeSomeone:
                BecomeFlower();
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
                GetComponent<Renderer>().material.SetFloat("_Glossiness", 1f);
                break;

            case MailType.Replacement:
                RandomMove();
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

    void BecomeFlower()
    {
        transform.localScale *= 0.3f;
        GetComponent<Renderer>().material.color = Color.magenta;
    }

    void FloatAway()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = Vector3.up * 5f;
        Destroy(gameObject, 3f);
    }

    void RandomMove()
    {
        transform.position += new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
    }

    void ConfettiAndDisappear()
    {
        gameObject.SetActive(false);
    }

    void ConfettiRain()
{
    ParticleSystem fx = Instantiate(confettiPrefab, transform.position + Vector3.up * 3f, Quaternion.identity);
    fx.Play();
    Destroy(fx.gameObject, 3f);
}

}
