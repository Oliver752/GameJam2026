using UnityEngine;
using System.Collections;


public class House : MonoBehaviour
{
    bool playerNear;
    bool spinning;
    public ParticleSystem confettiPrefab;
    public GameObject flowerPrefab;
    public GameObject treePrefab;


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

    void BecomeFlowerColor()
    {
        transform.localScale *= 0.3f;
        GetComponent<Renderer>().material.color = Color.magenta;
    }

    void BecomeFlower()
{
    SpawnReplacement(flowerPrefab);
}



    void FloatAway()
{
    Rigidbody rb = gameObject.AddComponent<Rigidbody>();
    rb.useGravity = false;
    rb.linearVelocity = Vector3.up * 2f;

    Destroy(gameObject, 8f); // bolo 3f
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
    if (confettiPrefab == null) return;

    // inštancuje presne tak, ako je prefab uložený v scéne alebo v projekte
    ParticleSystem fx = Instantiate(confettiPrefab); 
    fx.Play();
    Destroy(fx.gameObject, 5f);
}



void BecomeTree()
{
    SpawnReplacement(treePrefab);
}
IEnumerator ArtMode()
{
    Renderer r = GetComponent<Renderer>();
    for (int i = 0; i < 50; i++)
    {
        r.material.color = Random.ColorHSV();
        transform.Rotate(0, 15, 0);
        yield return new WaitForSeconds(0.1f);
    }
}
void SpawnReplacement(GameObject prefab)
{
    if (prefab == null) return;

    Vector3 pos = transform.position;
    Quaternion rot = transform.rotation;

    Destroy(gameObject);

    Instantiate(prefab, pos, rot);
}



}
