using UnityEngine;
using TMPro;

public class BinocularController : MonoBehaviour
{
    [Header("UI")]
    public GameObject binocularUI;       // Circular lens overlay
    public TextMeshProUGUI infoText;    // Info text for houses

    [Header("Camera Settings")]
    public Camera playerCamera;         // Main player camera
    public Vector3 defaultCamPos = new Vector3(0f, 1f, -3f);
    public Vector3 binocularCamPos = new Vector3(0f, 0.6f, 0.7f);
    public float defaultFOV = 60f;
    public float binocularFOV = 30f;
    public float transitionSpeed = 5f;   // Smoothness

    [Header("Raycast")]
    public float maxDistance = 100f;

    private bool binocularActive = false;
    private int rayLayerMask;

    private void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError("BinocularController: playerCamera is not assigned!");
            enabled = false;
            return;
        }

        binocularUI.SetActive(false);
        infoText.text = "";

        // Make sure camera starts at default
        playerCamera.transform.localPosition = defaultCamPos;
        playerCamera.fieldOfView = defaultFOV;
        rayLayerMask = ~LayerMask.GetMask("Player");
    }

    private void Update()
    {
        // Toggle binoculars
        if (Input.GetKeyDown(KeyCode.E))
        {
            binocularActive = !binocularActive;
            binocularUI.SetActive(binocularActive);

            if (!binocularActive)
                infoText.text = "";
        }

        // Smoothly move camera
        Vector3 targetPos = binocularActive ? binocularCamPos : defaultCamPos;
        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, targetPos, Time.deltaTime * transitionSpeed);

        // Smoothly adjust FOV
        float targetFOV = binocularActive ? binocularFOV : defaultFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);

        // Update house info while binoculars are active
        if (binocularActive)
            CheckHouseInView();
    }

    private void CheckHouseInView()
{
    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, rayLayerMask))
    {
        HouseInfo house = hit.collider.GetComponent<HouseInfo>();
        if (house != null)
        {
            infoText.text = house.GetHouseInfo();
            return;
        }
    }
    infoText.text = "";
}

}
